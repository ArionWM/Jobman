using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class JobManAttribute : JobDefinitionAttributeBase
    {
        public string PoolName { get; set; }
        public JobManAttribute()
        {

        }

        public JobManAttribute(string poolName)
        {
            if (string.IsNullOrWhiteSpace(poolName))
            {
                throw new ArgumentException($"'{nameof(poolName)}' cannot be null or whitespace.", nameof(poolName));
            }

            PoolName = poolName;

        }

        public override void Define(IWorkItemDefinition itemDefinition)
        {
            if (!string.IsNullOrEmpty(PoolName))
            {
                itemDefinition.Pool = this.PoolName;
            }
        }
    }
}
