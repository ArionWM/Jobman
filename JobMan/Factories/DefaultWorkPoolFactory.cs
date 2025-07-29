using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class DefaultWorkPoolFactory : IWorkPoolFactory
    {
        public IWorkPool Create(IWorkServer server, IWorkPoolOptions options)
        {
            return new WorkPool(server, options);
        }
    }
}
