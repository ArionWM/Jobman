using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JobMan.TestHelpers
{
    public class SynchronizedWorker : IWorker
    {
        protected ILogger logger;

        public Guid Id { get; protected set; }
        public bool IsDisposing { get; protected set; }

        public IWorkPool WorkPool { get; protected set; }

        public WorkerStatus Status { get; protected set; }

        public Thread Thread => null;

        public SynchronizedWorker(IWorkPool workPool, ThreadPriority priority = ThreadPriority.Normal)
        {
            this.WorkPool = workPool;
            this.Id = Guid.NewGuid();
            this.Status = WorkerStatus.Idle;
            this.logger = JobManGlobals.LoggerFactory.CreateLogger<SynchronizedWorker>();
        }


        public void Dispose()
        {
            
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        public void Start()
        {
            
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

        public void Execute(IWorkItem workItem)
        {
            Stopwatch sw = null;
            try
            {
                this.logger?.Log(LogLevel.Debug, $"Execute: {workItem.Definition.Data.MethodName}");

                //this.CurrentWorkItem = workItem;
                this.Status = WorkerStatus.Running;

                this.WorkPool.Options.JobExecutionFilter.PreExecute(this, workItem);

                sw = Stopwatch.StartNew();

                workItem.Definition.RetryCount++;

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Processing, 0, workItem.Definition.RetryCount, false);

                workItem.Job.Execute();

                sw.Stop();

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Completed, sw.ElapsedMilliseconds, workItem.Definition.RetryCount, true);

                this.WorkPool.Options.JobExecutionFilter.PostExecute(this, workItem);

                //this.CurrentWorkItem = null;


            }
            catch (Exception ex)
            {
                if (sw.IsRunning)
                    sw?.Stop();

                this.UpdateWorkItemExecutionState(workItem, WorkItemStatus.Fail, sw.ElapsedMilliseconds, workItem.Definition.RetryCount, true);

                JobExecutionFilterFailureResult ffresult = this.WorkPool.Options.JobExecutionFilter.Failure(this, workItem, ex, workItem.Definition.RetryCount);
                JobManGlobals.Server.Options.PolicyExecutor.ExecuteFailurePolicy(this, workItem, ex, workItem.Definition.RetryCount, ffresult);
            }
            finally
            {
                if (this.Status != WorkerStatus.WaitingStop)
                    this.Status = WorkerStatus.Idle;
            }
        }   

        public async Task StopAsync()
        {
            await Task.CompletedTask;
        }
    }
}
