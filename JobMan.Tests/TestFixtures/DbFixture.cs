using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests;

public class DbFixture : BasicFixture
{
    public IWorkItemStorage Storage { get; set; }
}
