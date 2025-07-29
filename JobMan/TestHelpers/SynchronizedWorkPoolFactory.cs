using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.TestHelpers
{
    
    public class SynchronizedWorkPoolFactory : IWorkPoolFactory
    {
        public IWorkPool Create(IWorkServer server, IWorkPoolOptions options)
        {
            return new SynchronizedWorkPool(server, options);
        }
    }
}
