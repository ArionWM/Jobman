using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class JobDefinitionAttributeBase : Attribute
    {
        public JobDefinitionAttributeBase()
        {

        }

        public abstract void Define(IWorkItemDefinition itemDefinition);

    }
}
