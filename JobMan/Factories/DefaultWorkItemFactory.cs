using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class DefaultWorkItemFactory : IWorkItemFactory
    {
        public IWorkItem Create(IWorkItemDefinition workItemDefinition)
        {
            IJob job = JobManGlobals.WorkServerOptions.JobFactory.Create(workItemDefinition.Data);
            IWorkItem workItem = new WorkItem(workItemDefinition, job);
            return workItem;
        }
    }
}
