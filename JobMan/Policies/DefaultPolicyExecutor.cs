using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Policies
{
    public class DefaultPolicyExecutor : IJobExecutionFilter, IPolicyExecutor
    {
        public int Index { get; set; } = 100000;

        public void Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount, ref JobExecutionFilterFailureResult filterFailureResult)
        {
            IExecutionPolicy ePol = worker.WorkPool.Options.ExecutionPolicy;

            if (filterFailureResult.Handled)
                return;

            //int _retryCount = retryCount + 1;
            filterFailureResult.ReTry = retryCount < ePol.FailureRetryCount;
        }

        public void ExecuteFailurePolicy(IWorker worker, IWorkItem item, Exception ex, int retryCount, JobExecutionFilterFailureResult ffres)
        {
            if (ffres.ReTry)
            {
                IExecutionPolicy ePol = worker.WorkPool.Options.ExecutionPolicy;

                int delayMs = 100;
                if (ePol.FailureRetryWaitTimes != null || ePol.FailureRetryWaitTimes.Length > 1)
                {
                    if (ePol.FailureRetryWaitTimes.Length > item.Definition.RetryCount - 1)
                        delayMs = ePol.FailureRetryWaitTimes[item.Definition.RetryCount - 1];
                    else
                        delayMs = ePol.FailureRetryWaitTimes.Last();
                }

                item.Definition.Status = WorkItemStatus.Enqueuing;
                worker.WorkPool.UpdateStatus(item);

                Thread.Sleep(delayMs);

                //item.Definition.RetryCount++;
                item.Definition.Status = WorkItemStatus.Enqueued;
                worker.WorkPool.UpdateStatus(item);
                worker.WorkPool.EnqueueDirect(item);
            }
            else
            {
                //Do nothing
            }
        }

        public void PostExecute(IWorker worker, IWorkItem item)
        {

        }

        public void PreExecute(IWorker worker, IWorkItem item)
        {

        }
    }
}
