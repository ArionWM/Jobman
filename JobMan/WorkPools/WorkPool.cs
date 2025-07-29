using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace JobMan
{
    public class WorkPool : IWorkPool, IJobExecutionFilter
    {


        private bool disposedValue;
        private DateTime _lastMetricUpdate = DateTime.MinValue;

        protected ConcurrentDictionary<Guid, IWorker> _workers = new ConcurrentDictionary<Guid, IWorker>();

        protected ConcurrentQueue<IWorkItem> _preProcessBuffer = new ConcurrentQueue<IWorkItem>();

        protected Thread _managerThread;

        protected Thread _metricsThread;

        protected IWorkServer _server;
        protected IWorker[] EnabledWorkers { get { return this.Workers.Where(wrk => wrk.Status != WorkerStatus.Terminated).ToArray(); } }
        protected IWorker[] ActiveWorkers { get { return this.Workers.Where(wrk => wrk.Status != WorkerStatus.Terminated && wrk.Status != WorkerStatus.Stopped).ToArray(); } }
        protected ConcurrentQueue<IWorkItem> PreProcessBuffer => _preProcessBuffer;

        protected ILogger logger;

        public int Index { get; set; } = 1000;

        public IWorkPoolOptions Options { get; protected set; }
        public string Name { get; protected set; }

        public int EnabledWorkerCount { get { return this.Workers.Count(wrk => wrk.Status != WorkerStatus.Terminated && !wrk.IsDisposing); } }

        public IEnumerable<IWorker> Workers => _workers.Values;




        public WorkPoolStatus Status { get; protected set; }
        public WorkPoolMetrics Metrics { get; }


        public event Action<IWorkPool> MetricsUpdated;

        public WorkPool(IWorkServer server, IWorkPoolOptions options)
        {
            this.logger = JobManGlobals.LoggerFactory.CreateLogger<WorkPool>();

            this._server = server;
            this.Options = options;
            this.Name = options.Name;
            this.Status = WorkPoolStatus.Stopped;
            this.Metrics = new WorkPoolMetrics(this.Name);

            _managerThread = new Thread(this.ManagerThread);
            _managerThread.IsBackground = true;
            _managerThread.Priority = this.Options.Priority;
            _managerThread.Start();

            _metricsThread = new Thread(this.MetricsThread);
            _metricsThread.IsBackground = true;
            _metricsThread.Priority = ThreadPriority.Lowest;
            _metricsThread.Start();

            this.Metrics.DataShift += Metrics_DataShift;

            this.CheckStorage();
        }

        public void EnqueueDirect(IWorkItem item)
        {
            this.PreProcessBuffer.Enqueue(item);
        }

        public bool CanEnqueueDirect(IWorkItemDefinition item)
        {
#if DEBUG
            this.logger.LogDebug($"Workpool / CanEnqueueDirect ({this.Name}); {item.Id} / {item.Data.MethodName}");
#endif


            if (this.PreProcessBuffer.Count < this.Options.PreProcessBufferLenght && item.Pool == this.Name)
            {

#if DEBUG
                this.logger.LogDebug($"Workpool / CanEnqueueDirect ({this.Name}); taken");
#endif

                item.Status = WorkItemStatus.Enqueued;

                IWorkItem witem = this._server.Options.WorkItemFactory.Create(item);
                this.EnqueueDirect(witem);

                return true;
            }
            else
            {

#if DEBUG
                this.logger.LogDebug($"Workpool / CanEnqueueDirect ({this.Name}); not taken");
#endif

                return false;
            }
        }

        protected void CheckPreProcessBuffer()
        {
            if (this.PreProcessBuffer.Count < this.Options.PreProcessBufferLenght)
            {
                int diff = this.Options.PreProcessBufferLenght - this.PreProcessBuffer.Count;
                IWorkItemDefinition[] items = this.Options.Storage.PeekOrWait(diff, this.Options.Name, Convert.ToInt32(this.Options.IdlePeriod.TotalMilliseconds), CancellationToken.None);
                foreach (IWorkItemDefinition item in items)
                    try
                    {
#if DEBUG
                        this.logger.LogDebug($"Workpool ({this.Name}), peek from storage: {item.Id}");
#endif

                        IWorkItem witem = this._server.Options.WorkItemFactory.Create(item);
                        this.PreProcessBuffer.Enqueue(witem);
                    }
                    catch (Exception ex)
                    {
                        //TODO: Log
                        this.DoFail(item, ex);
                    }

            }

            this.Metrics.SetWaiting(this.PreProcessBuffer.Count);
        }

        protected async Task DoMetricsUpdatedEvent()
        {
            this.MetricsUpdated?.Invoke(this);
            await Task.CompletedTask;
        }

        protected void DoFail(IWorkItemDefinition itemDefinition, Exception ex)
        {
            this.logger.LogError(ex, $"Workpool ({this.Name}), FAIL; {itemDefinition.Id}");

            itemDefinition.Status = WorkItemStatus.Fail;
            itemDefinition.Description = ex.Message;
            this.Options.Storage.UpdateStatus(itemDefinition);
        }

        protected void Metrics_DataShift()
        {
            _ = this.DoMetricsUpdatedEvent();
        }

        protected IWorker AddWorker()
        {
#if DEBUG
            this.logger.LogDebug($"Workpool ({this.Name}), peek from storage; creating new worker (available: {this._workers.Count})");
#endif

            IWorker worker = new Worker(this, this.Options.Priority);
            this._workers.TryAdd(worker.Id, worker);

            worker.Start();
            return worker;
        }

        protected async void RemoveWorker(IWorker worker)
        {
            try
            {
#if DEBUG
                this.logger.LogDebug($"Workpool ({this.Name}), peek from storage; removing worker: {worker.Id}");
#endif


                worker.DisposeAsync().Wait();
                this._workers.TryRemove(worker.Id, out _);
                //if (!this._workers.TryRemove(worker.Id, out _))
                //    throw new InvalidOperationException("Worker can't remove"); //
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        protected void CheckWorkers()
        {
            while (this._workers.Count < this.Options.ThreadCount)
            {
                this.AddWorker();
                this.UpdateMetrics();
            }

            if (this.EnabledWorkerCount > this.Options.ThreadCount)
            {
                IWorker worker = this.Workers.FirstOrDefault(wrk => wrk.Status != WorkerStatus.Terminated && !wrk.IsDisposing);
                if (worker != null)
                {
                    this.RemoveWorker(worker);
                    this.UpdateMetrics();
                }
            }
        }

        protected void UpdateMetrics()
        {
            if (_lastMetricUpdate != JobManGlobals.Time.Now.WithSecond())
            {

                this.Metrics.WorkerCount = this.Workers.Count(wrk => wrk.Status.IsActive());
                this.Metrics.Status = this.Status;
                _ = this.Metrics.Add(0, 0, this._preProcessBuffer.Count);
                _lastMetricUpdate = JobManGlobals.Time.Now.WithSecond();
            }
        }

        protected void ManagerThread()
        {
            try
            {
                //TODO: Lock/SingleRun
                while (this.Status != WorkPoolStatus.Terminated)
                {
                    if (this.Status == WorkPoolStatus.Active)
                    {
                        this.CheckPreProcessBuffer();
                        this.CheckWorkers();
                    }

                    this.UpdateMetrics();
                    if (this.Status == WorkPoolStatus.Stopped)
                        Thread.Sleep(1000);
                    else
                        Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(1000);
                //TODO: Log !!!
            }
        }

        protected void MetricsThread()
        {
            try
            {
                while (this.Status != WorkPoolStatus.Terminated)
                {

                    this.UpdateMetrics();
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                //Thread.Sleep(1000);
                //TODO: Log !!!
            }
        }

        object _testLocker = new object();

        public IWorkItem GetWorkItemOrWait(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            do
            {
                lock (_testLocker)
                {
                    if (this.PreProcessBuffer.Count > 0)
                    {
                        IWorkItem wItem;
                        if (this.PreProcessBuffer.TryDequeue(out wItem))
                            return wItem;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (this.Status != WorkPoolStatus.Active)
                    return null;

                if (cancellationToken.WaitHandle.WaitOne(100))
                    return null;

            }
            while (true);

        }

        public void UpdateStatus(IWorkItem workItem)
        {
            this.Options.Storage.UpdateStatus(workItem.Definition);
        }

        public async Task StartAsync()
        {
#if DEBUG
            this.logger.LogDebug($"Workpool ({this.Name}), starting");
#endif

            if (this.Status != WorkPoolStatus.Active)
            {
                this.Status = WorkPoolStatus.Active;

                if (this.Options.Storage == null)
                    throw new InvalidOperationException("Storage is not set");

                this._server.Options.JobExecutionFilter.Add(this);

                this.Options.Storage.RegisterDirectEnqueueCheck(this);

                this.CheckWorkers();
                this.UpdateMetrics();
            }

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
#if DEBUG
            this.logger.LogDebug($"Workpool ({this.Name}), stopping");
#endif

            this.Status = WorkPoolStatus.WaitingStop;
            bool allStopped = false;
            do
            {
                Thread.Sleep(50);

                if (this.PreProcessBuffer.Count > 0)
                {
                    //Do nothing
                }
                else
                {
                    foreach (IWorker worker in this.EnabledWorkers)
                    {
                        _ = worker.StopAsync();
                    }

                    allStopped = this.Workers.All(wrk => wrk.Status == WorkerStatus.Stopped);
                }
            }
            while (!allStopped);
            await Task.CompletedTask;

            this._server.Options.JobExecutionFilter.Remove(this);
            this.Status = WorkPoolStatus.Stopped;
            this.UpdateMetrics();

        }


        public void PreExecute(IWorker worker, IWorkItem item)
        {

        }

        public void PostExecute(IWorker worker, IWorkItem item)
        {
            if (worker.WorkPool == this)
                _ = this.Metrics.Add(1, 0, this._preProcessBuffer.Count);
        }

        public void Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount, ref JobExecutionFilterFailureResult filterFailureResult)
        {
            if (worker.WorkPool == this)
            {
                _ = this.Metrics.Add(0, 1, this._preProcessBuffer.Count);
                this.DoFail(item.Definition, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.Status = WorkPoolStatus.Terminated;
                    _managerThread.Join();
                    _metricsThread.Join();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
