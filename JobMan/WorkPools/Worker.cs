using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ThreadState = System.Threading.ThreadState;

namespace JobMan
{
    public class Worker : IWorker
    {
        protected ILogger logger;
        protected IWorkItem CurrentWorkItem { get; private set; }
        

        public Guid Id { get; protected set; }
        public bool IsDisposing { get; protected set; }
        public Thread Thread { get; protected set; }

        public IWorkPool WorkPool { get; protected set; }

        public WorkerStatus Status { get; protected set; }

        //public event Action<IWorker, IWorkItem> Executing;
        //public event Action<IWorker, IWorkItem> Executed;
        //public event Action<IWorker, IWorkItem, Exception> Failure;
        //TODO: Logger

        public Worker(IWorkPool workPool, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.WorkPool = workPool;
            this.Id = Guid.NewGuid();
            this.Status = WorkerStatus.Stopped;
            this.Thread = new Thread(this.WorkerProcess);
            this.Thread.IsBackground = true;
            this.Thread.Priority = priority;
            this.logger = JobManGlobals.LoggerFactory.CreateLogger<Worker>();

        }

        protected void UpdateWorkItemExecutionState(IWorkItem workItem, WorkItemStatus status, long processTimeMs, int retryCount, bool reschedule)
        {
            workItem.Definition.Status = status;
            workItem.Definition.LastExecuteTime = JobManGlobals.Time.Now;
            workItem.Definition.ProcessTimeMs = processTimeMs;
            workItem.Definition.RetryCount = retryCount;
            //workItem.RetryCount = retryCount;

            //if (workItem.Definition.Type == WorkItemType.RecurrentRun) //TODO: Abstraction?
            //{
            //    workItem.Definition.CalculateNextRun();
            //    workItem.Definition.Status = WorkItemStatus.WaitingProcess;
            //}

            this.WorkPool.UpdateStatus(workItem);
        }

        protected void Execute(IWorkItem workItem)
        {
            Stopwatch sw = null;
            try
            {
                this.logger?.Log(LogLevel.Debug, $"Execute: {workItem.Definition.Data.MethodName}");

                this.CurrentWorkItem = workItem;
                this.Status = WorkerStatus.Running;

                this.WorkPool.Options.JobExecutionFilter.PreExecute(this, workItem);

                sw = Stopwatch.StartNew();

                workItem.Definition.RetryCount++;

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Processing, 0, workItem.Definition.RetryCount, false);

                workItem.Job.Execute();

                sw.Stop();

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Completed, sw.ElapsedMilliseconds, workItem.Definition.RetryCount, true);

                this.WorkPool.Options.JobExecutionFilter.PostExecute(this, workItem);

                this.CurrentWorkItem = null;


            }
            catch (Exception ex)
            {
                if (sw.IsRunning)
                    sw?.Stop();

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Fail, sw.ElapsedMilliseconds, workItem.Definition.RetryCount, true);

                throw ex;
            }
            finally
            {
                if (this.Status != WorkerStatus.WaitingStop)
                    this.Status = WorkerStatus.Idle;
            }

        }

        protected void WorkerProcess()
        {
            try
            {
                while (this.Status != WorkerStatus.Terminated)
                {
                    IWorkItem workItem = null;
                    try
                    {
                        if (this.Status == WorkerStatus.Idle)
                        {
                            workItem = this.WorkPool.GetWorkItemOrWait(CancellationToken.None);
                            if (workItem == null)
                            {
                                Debug.WriteLine($"Worker waiting {this.Id}");
                                Thread.Sleep(100); //Parametrik olmalı; Worker idle timeout
                            }
                            else
                            {
                                this.Execute(workItem);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100); //Parametrik olmalı; Worker idle timeout
                        }
                    }
                    catch (ThreadAbortException) //TODO: Thread.Abort is obsolete; https://learn.microsoft.com/tr-tr/dotnet/core/compatibility/core-libraries/5.0/thread-abort-obsolete
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        //FailureRetryCount
                        this.Status = WorkerStatus.Idle;
                        this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Fail, 0, workItem.Definition.RetryCount, true);

                        JobExecutionFilterFailureResult ffresult = this.WorkPool.Options.JobExecutionFilter.Failure(this, workItem, ex, workItem.Definition.RetryCount);
                        JobManGlobals.Server.Options.PolicyExecutor.ExecuteFailurePolicy(this, workItem, ex, workItem.Definition.RetryCount, ffresult);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing
            }
        }

        public void Start()
        {
            this.Status = WorkerStatus.Idle;
            if (this.Thread.ThreadState.HasFlag(ThreadState.Unstarted))
                this.Thread.Start();
        }

        public async Task StopAsync()
        {
            this.Status = WorkerStatus.WaitingStop;

            do
            {
                Thread.Sleep(50);
            }
            while (this.CurrentWorkItem != null);

            this.Status = WorkerStatus.Stopped;

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            this.IsDisposing = true;
            this.StopAsync().Wait();
            this.Thread.Abort();
            this.Status = WorkerStatus.Terminated;
            this.IsDisposing = false;
        }

        public async Task DisposeAsync()
        {
            this.IsDisposing = true;
            this.StopAsync().Wait();
            this.Status = WorkerStatus.Terminated;
            this.IsDisposing = false;

            await Task.CompletedTask;
        }
    }
}
