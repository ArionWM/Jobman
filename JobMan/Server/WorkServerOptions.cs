

using JobMan.Policies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class WorkServerOptions : IWorkServerOptions
    {
        public ITypeResolver TypeResolver { get; set; }
        public IWorkItemDefinitionSerializer WorkItemDefinitionSerializer { get; set; }
        public IWorkItemFactory WorkItemFactory { get; set; }
        public IWorkItemDefinitionFactory WorkItemDefinitionFactory { get; set; }
        public IJobFactory JobFactory { get; set; }

        public IWorkPoolFactory WorkPoolFactory { get; set; }

        public List<IWorkPoolOptions> PoolOptions { get; set; } = new List<IWorkPoolOptions>();

        //public ILogger<WorkServer> Logger { get; set; }

        public IExecutionPolicy DefaultPolicy { get; set; }

        public IWorkPoolOptions DefaultPoolOptions => this[WorkPoolOptions.POOL_DEFAULT];

        public IWorkPoolOptions this[string name] => this.PoolOptions.FirstOrDefault(po => po.Name == name);

        public IPolicyExecutor PolicyExecutor { get; set; }

        public IJobFilterManager JobExecutionFilter { get; set; }

        public string CleanJobsScheduleCron { get; set; }

        public bool DelayedStart { get; set; }

        public int DelayedStartMilliseconds { get; set; }

        public WorkServerOptions()
        {
            this.CleanJobsScheduleCron = "30 00 * * *";
            this.TypeResolver = new DefaultTypeResolver(); //Bunları da DI servislerine çevirmeli mi? Hep ya da hiç?
            this.JobFactory = new DefaultJobFactory();
            this.WorkItemFactory = new DefaultWorkItemFactory();
            this.WorkPoolFactory = new DefaultWorkPoolFactory();
            this.WorkItemDefinitionFactory = new DefaultWorkItemDefinitionFactory();
            this.WorkItemDefinitionSerializer = new DefaultWorkItemDefinitionSerializer();
            this.DefaultPolicy = new ExecutionPolicy();
            this.JobExecutionFilter = new JobFilterManager();
            this.JobExecutionFilter.Add(new DefaultPolicyExecutor());
            this.PolicyExecutor = new DefaultPolicyExecutor();
            this.DelayedStart = true;
            this.DelayedStartMilliseconds = 2000; //1 second
            //this.AddPool(WorkPoolOptions.POOL_DEFAULT);
        }
    }
}
