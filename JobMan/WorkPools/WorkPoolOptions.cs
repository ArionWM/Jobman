using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class WorkPoolOptions : IWorkPoolOptions
    {
        public const string POOL_DEFAULT = "Default";


        public string Name { get; set; }
        public int ThreadCount { get; set; }
        public int PreProcessBufferLenght { get; set; }

        public StorageOptions StorageOptions { get; set; }
        public IWorkItemStorage? Storage { get; set; }
        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan IdlePeriod { get; set; } = TimeSpan.FromSeconds(30);
        public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
        public int FailureRetryCount { get; set; } = 3;

        public bool ShowInUi { get; set; } = true;

        //public ILogger Logger { get; set; }
        public IExecutionPolicy ExecutionPolicy { get; set; }

        public IJobFilterManager JobExecutionFilter { get; set; }

        public event Action<IWorker, IWorkItem> Executing;
        public event Action<IWorker, IWorkItem> Executed;
        public event Action<IWorker, IWorkItem> Failure;

        public WorkPoolOptions()
        {
            this.ThreadCount = Environment.ProcessorCount / 2;
            this.PreProcessBufferLenght = this.ThreadCount * 2;
        }
    }
}
