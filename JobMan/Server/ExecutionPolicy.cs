using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    internal class ExecutionPolicy : IExecutionPolicy
    {
        public int FailureRetryCount { get; set; } = 3;
        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.MaxValue;
        public int[] FailureRetryWaitTimes { get; set; }

        public ExecutionPolicy()
        {
            this.FailureRetryWaitTimes = new int[1] { 1000 };
        }
    }
}
