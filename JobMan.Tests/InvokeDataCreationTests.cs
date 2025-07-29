using JobMan.Tests.Mock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests
{
    public class InvokeDataCreationTests : IClassFixture<BasicFixture>
    {
        BasicFixture fixture;
        //TODO: to config

        public InvokeDataCreationTests(BasicFixture fixture)
        {
            this.fixture = fixture;
        }


#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void SampleAction1()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void SampleAction2(string arg0, int arg1)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void SampleAction3(string arg0, int? arg1)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void SampleAction4(Guid arg0, int? arg1)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void IncorrectComplexArgumentedAction(IWorkPool complexArg)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {

        }

        private static void IncorrectStaticAction()
        {

        }

        private void IncorrectInstanceAction()
        {

        }

        [Fact]
        public void CorrectExpressionSerialization1()
        {
            Expression<Action> expression = () => SampleAction1();
            InvokeData actionData = this.fixture.ToInvokeData(expression);
            Assert.NotNull(actionData);
            Assert.Equal("SampleAction1", actionData.MethodName);
            Assert.Empty(actionData.ArgumentValues);
            Assert.Equal("JobMan.Tests.InvokeDataCreationTests", actionData.ClassType);
        }

        [Fact]
        public void CorrectExpressionSerialization2()
        {
            Expression<Action> expression1 = () => SampleAction2("value0", 1);
            InvokeData actionData1 = this.fixture.ToInvokeData(expression1);
            Assert.NotNull(actionData1);
            Assert.Equal("SampleAction2", actionData1.MethodName);
            Assert.Equal(2, actionData1.ArgumentValues.Length);
            Assert.Equal("JobMan.Tests.InvokeDataCreationTests", actionData1.ClassType);
            Assert.Equal("value0", actionData1.ArgumentValues[0]);
            Assert.Equal(1, actionData1.ArgumentValues[1]);

            Guid id = Guid.NewGuid();
            Expression<Action> expression2 = () => SampleAction4(id, 3);
            InvokeData actionData2 = this.fixture.ToInvokeData(expression2);
            Assert.NotNull(actionData2);
            Assert.Equal("SampleAction4", actionData2.MethodName);
            Assert.Equal(2, actionData2.ArgumentValues.Length);
            Assert.Equal("JobMan.Tests.InvokeDataCreationTests", actionData2.ClassType);
            Assert.Equal(id, actionData2.ArgumentValues[0]);
            Assert.Equal(3, actionData2.ArgumentValues[1]);


        }

        //
        [Fact]
        public void NullablesSupported()
        {
            Expression<Action> expression = () => SampleAction3("value0", 1);
            InvokeData actionData = this.fixture.ToInvokeData(expression);
            Assert.NotNull(actionData);
            Assert.Equal("SampleAction3", actionData.MethodName);
            Assert.Equal(2, actionData.ArgumentValues.Length);
            Assert.Equal("JobMan.Tests.InvokeDataCreationTests", actionData.ClassType);
            Assert.Equal("value0", actionData.ArgumentValues[0]);
            Assert.Equal(1, actionData.ArgumentValues[1]);
        }

        [Fact]
        public void InstanceMethodsNotSupported()
        {
            Expression<Action> expression = () => IncorrectInstanceAction();
            Assert.Throws<NotSupportedException>(() =>
            {
                InvokeData actionData = this.fixture.ToInvokeData(expression);
            });
        }

        [Fact]
        public void PrivateMethodsNotSupported()
        {
            Expression<Action> expression = () => IncorrectStaticAction();
            Assert.Throws<NotSupportedException>(() =>
            {
                InvokeData actionData = this.fixture.ToInvokeData(expression);
            });
        }

        //[Fact]
        //public void ComplexArgumentsNotSupported()
        //{
        //    Expression<Action> expression = () => IncorrectComplexArgumentedAction(new MockWorkPool());
        //    Assert.Throws<NotSupportedException>(() =>
        //    {
        //        InvokeData actionData = this.fixture.ToInvokeData(expression);
        //    });
        //}

        [Fact]
        void WorkItemDefinitionCreation()
        {


            Expression<Action> exSampleAction1 = () => SampleAction1();
            Expression<Action> exSampleAction2 = () => SampleAction2("myValue0", 2);

            IWorkItemDefinition wid1 = JobManGlobals.WorkServerOptions.WorkItemDefinitionFactory.Create(exSampleAction1);
            IWorkItemDefinition wid2 = JobManGlobals.WorkServerOptions.WorkItemDefinitionFactory.Create(exSampleAction2);

            Assert.Equal(WorkItemStatus.WaitingProcess, wid1.Status);

            Assert.Equal(typeof(InvokeDataCreationTests).FullName, wid1.Data.ClassType);
            Assert.Equal(nameof(InvokeDataCreationTests.SampleAction1), wid1.Data.MethodName);

            Assert.Equal(typeof(string).FullName, wid2.Data.PropertyTypes[0]);
            Assert.Equal(typeof(int).FullName, wid2.Data.PropertyTypes[1]);

            Assert.Equal("myValue0", wid2.Data.ArgumentValues[0]);
            Assert.Equal(2, wid2.Data.ArgumentValues[1]);

        }

    }
}
