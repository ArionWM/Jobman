using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JobMan.TestHelpers
{
    /// <summary>
    /// For test purposes...
    /// </summary>


    public class SynchronizedWorkPool : IWorkPool
    {

        private bool disposedValue;



        protected IWorkServer _server;

        public IWorkPoolOptions Options { get; protected set; }
        public string Name { get; protected set; }

        public int EnabledWorkerCount { get { return 1; } }

        public IEnumerable<IWorker> Workers => null;
        public ConcurrentQueue<IWorkItem> PreProcessBuffer => null;

        public WorkPoolStatus Status { get; protected set; }
        public WorkPoolMetrics Metrics { get; }
        public event Action<IWorkPool> MetricsUpdated;

        public SynchronizedWorkPool(IWorkServer server, IWorkPoolOptions options)
        {
            this._server = server;
            this.Options = options;
            this.Name = options.Name;
            this.Status = WorkPoolStatus.Stopped;
            this.Metrics = new WorkPoolMetrics(this.Name);

            this.Metrics.DataShift += Metrics_DataShift;

            this.CheckStorage();
        }

        private void Execute(IWorkItem workItem)
        {
            try
            {
                SynchronizedWorker worker = new SynchronizedWorker(this, this.Options.Priority);
                worker.Execute(workItem);

            }
            finally
            {

            }
        }

        public void EnqueueDirect(IWorkItem item)
        {
            this.Execute(item);
        }




        protected async Task DoMetricsUpdatedEvent()
        {
            this.MetricsUpdated?.Invoke(this);
            await Task.CompletedTask;
        }

        protected void DoFail(IWorkItemDefinition itemDefinition, Exception ex)
        {
            itemDefinition.Status = WorkItemStatus.Fail;
            itemDefinition.Description = ex.Message;
            this.Options.Storage.UpdateStatus(itemDefinition);
        }

        protected void Metrics_DataShift()
        {
            _ = this.DoMetricsUpdatedEvent();
        }


        public IWorkItem GetWorkItemOrWait(CancellationToken cancellationToken)
        {
            return null;

        }

        public void UpdateStatus(IWorkItem workItem)
        {
            this.Options.Storage.UpdateStatus(workItem.Definition);
        }


        public async Task StartAsync()
        {
            this.Status = WorkPoolStatus.Active;

            if (this.Options.Storage == null)
                throw new InvalidOperationException("Storage is not set");

            this.Options.Storage.RegisterDirectEnqueueCheck(this);

            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            this.Status = WorkPoolStatus.WaitingStop;
            this.Status = WorkPoolStatus.Stopped;
            await Task.CompletedTask;
        }

        public bool CanEnqueueDirect(IWorkItemDefinition item)
        {
            item.Status = WorkItemStatus.Enqueued;

            IWorkItem witem = this._server.Options.WorkItemFactory.Create(item);
            this.Execute(witem);

            return true;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Status = WorkPoolStatus.Terminated;
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
