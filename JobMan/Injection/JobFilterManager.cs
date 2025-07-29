using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{


    public class JobFilterManager : FilterManager<IJobExecutionFilter>, IJobFilterManager
    {
        public JobExecutionFilterFailureResult Failure(IWorker worker, IWorkItem item, Exception ex, int remainingRetryCount)
        {
            JobExecutionFilterFailureResult filterFailureResult = new JobExecutionFilterFailureResult();
            var filters = GetFilters();
            foreach (var filter in filters)
            {
                filter.Failure(worker, item, ex, remainingRetryCount, ref filterFailureResult);
            }

            return filterFailureResult;
        }

        public void PreExecute(IWorker worker, IWorkItem item)
        {
            var filters = GetFilters();
            foreach (var filter in filters)
            {
                filter.PreExecute(worker, item);
            }
        }

        public void PostExecute(IWorker worker, IWorkItem item)
        {
            var filters = GetFilters();
            foreach (var filter in filters)
            {
                filter.PostExecute(worker, item);
            }
        }


    }
}
