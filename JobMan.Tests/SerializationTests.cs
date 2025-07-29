using JobMan.Tests.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests
{
#pragma warning disable xUnit1013 // Public method should be marked as test
    public class SerializationTests : IClassFixture<BasicFixture>
    {
        BasicFixture fixture;

        public SerializationTests(BasicFixture fixture)
        {
            this.fixture = fixture;
        }



        public static void TestMethod1(TestDto1 dto1)
        {

        }

        public static void TestMethod2(ITestDto1 dto2)
        {

        }

        [Fact]
        public void Check1()
        {
            TestDto1 dto1 = new TestDto1();
            dto1.Name = "Test1";

            InvokeData idata1 = this.fixture.ToInvokeData(() => TestMethod1(dto1));
            string json1 = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.ToJson(idata1);
            InvokeData idata1_c = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.FromJson(json1);

            Assert.IsType<TestDto1>(idata1_c.ArgumentValues[0]);
            Assert.Equal(dto1.Name, ((TestDto1)idata1_c.ArgumentValues[0]).Name);

            ITestDto1 dto2 = new TestDto1();
            dto2.Name = "Test2";

            InvokeData idata2 = this.fixture.ToInvokeData(() => TestMethod2(dto2));
            string json2 = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.ToJson(idata2);
            InvokeData idata2_c = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.FromJson(json2);

            Assert.IsType<TestDto1>(idata2_c.ArgumentValues[0]);
            Assert.Equal(dto2.Name, ((TestDto1)idata2_c.ArgumentValues[0]).Name);
        }
    }

#pragma warning restore xUnit1013 // Public method should be marked as test
}
