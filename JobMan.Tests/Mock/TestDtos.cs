using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests.Mock
{
    public interface ITestDto1
    {
        string Name { get; set; }
    }

    public class TestDto1 : ITestDto1
    {
        public string Name { get; set; }
    }
}
