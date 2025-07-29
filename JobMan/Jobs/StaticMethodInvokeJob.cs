using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobMan.Jobs
{
    public class StaticMethodInvokeJob : IJob
    {
        protected virtual MethodInfo MethodInfo { get; }
        protected virtual object[] ParameterValues { get; }
        public virtual Guid Id {get; protected set;}
        

        public StaticMethodInvokeJob(MethodInfo methodInfo, object[] parameterValues)
        {
            Id = Guid.NewGuid();
            MethodInfo = methodInfo;
            ParameterValues = parameterValues;
        }

        public void Execute()
        {
            this.MethodInfo.Invoke(null, this.ParameterValues);
        }
    }
}
