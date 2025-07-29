using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class WorkItem : IWorkItem
    {
        public IJob Job { get; protected set; }

        public IWorkItemDefinition Definition { get; protected set; }

        public IWorkItemStorage Storage { get; protected set; } //Storage?

        //public WorkItem(IWorkItemDefinition definition)
        //{
        //    this.Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        //    this.Job = DefaultJobFactory.Instance.Create(definition.Data);
        //}

        public WorkItem(IWorkItemDefinition definition, IJob job)
        {
            this.Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.Job = job;
        }
    }
}
