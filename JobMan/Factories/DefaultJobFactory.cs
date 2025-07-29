using JobMan.Jobs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JobMan
{
    public class DefaultJobFactory : IJobFactory
    {

        public IJob Create(InvokeData invokeData)
        {
            Type type = JobManGlobals.WorkServerOptions.TypeResolver.Get(invokeData.ClassType);
            if (type == null)
                throw new InvalidOperationException($"'{invokeData.ClassType}' type not found");

            Type[] parameterTypes = invokeData.PropertyTypes.Select(pt => JobManGlobals.WorkServerOptions.TypeResolver.Get(pt)).ToArray() ?? new Type[0];
            MethodInfo methodInfo = type.GetMethod(invokeData.MethodName, parameterTypes);

            StaticMethodInvokeJob job = new StaticMethodInvokeJob(methodInfo, invokeData.ArgumentValues);
            return job;
        }
    }
}
