using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace JobMan
{
    public class WorkItemDefinition : IWorkItemDefinition
    {
        public WorkItemType Type { get; set; }
        public Guid Schedule { get; set; }
        public long Id { get; set; }
        public string Pool { get; set; }
        public DateTime LastExecuteTime { get; set; }
        public DateTime NextExecuteTime { get; set; }
        public string Cron { get; set; }
        public InvokeData Data { get; set; }

        public string Tag { get; set; }

        public WorkItemStatus Status { get; set; }
        public long ProcessTimeMs { get; set; }
        public string Description { get; set; }
        public int RetryCount { get; set; }

        public override string ToString()
        {
            return this.Pool + ", " + this.Type.ToString() + ", " + this.Data?.ToString();
        }

        public IWorkItemDefinition Clone()
        {
            WorkItemDefinition clone = new WorkItemDefinition();
            clone.Id = this.Id;
            clone.Type = this.Type;
            clone.Schedule = this.Schedule;
            clone.Pool = this.Pool;
            clone.LastExecuteTime = this.LastExecuteTime;
            clone.NextExecuteTime = this.NextExecuteTime;
            clone.Cron = this.Cron;
            clone.Data = this.Data;
            clone.Status = this.Status;
            clone.ProcessTimeMs = this.ProcessTimeMs;
            return clone;
        }
    }
}
