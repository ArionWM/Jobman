using JobMan.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests.Mock
{
    internal class TestTimeResolver : DefaultTimeResolver
    {
        DateTime time;
        public override DateTime Now => time;


        public TestTimeResolver(DateTime time)
        {
            this.time = time;   
        }
    }
}
