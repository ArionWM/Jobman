using JobMan.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests
{
    public class BasicFixture : IDisposable
    {
        public BasicFixture()
        {
            JobManGlobals.WorkServerOptions = new WorkServerOptions(); //Set default options
            JobManGlobals.Time = new DefaultTimeResolver();
        }

        public InvokeData ToInvokeData(Expression<Action> expression)
        {
            IWorkItemDefinition def = JobManGlobals.WorkServerOptions.WorkItemDefinitionFactory.Create(expression);
            return def.Data;
        }

        public virtual void Dispose()
        {

        }
    }
}
