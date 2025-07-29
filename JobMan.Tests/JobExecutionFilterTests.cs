using JobMan.Storage.MemoryStorage;
using JobMan.Tests.Mock;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobMan.TestHelpers;

namespace JobMan.Tests
{
#pragma warning disable xUnit1013 // Public method should be marked as test
    public class JobExecutionFilterTests : IClassFixture<BasicFixture>
    {
        public class TestException : Exception
        {
            public TestException(string message) : base(message)
            {

            }

        }

        internal class JobExecutionFilter : IJobExecutionFilter
        {
            public int Index { get; set; } = 200;

            public IWorkItem LastWorkItem { get; set; }

            public void Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount, ref JobExecutionFilterFailureResult filterFailureResult)
            {
                this.LastWorkItem = item;
            }

            public void PostExecute(IWorker worker, IWorkItem item)
            {
                this.LastWorkItem = item;

            }

            public void PreExecute(IWorker worker, IWorkItem item)
            {
                this.LastWorkItem = item;

            }
        }


        BasicFixture fixture;
        public JobExecutionFilterTests()
        {
            this.fixture = fixture;
        }


        public static void SampleFailureAction1(string message)
        {
            //InvokedMethods.Add(nameof(SampleAction1));

            throw new TestException(message);
        }



        [Fact]
        public void JobExecutionFilterTests_PreExecute()
        {

            try
            {
                JobExecutionFilter filter = new JobExecutionFilter();

                //InvokedMethods.Clear();

                var services = new ServiceCollection();
                services.AddSingleton<ILoggerFactory, NullLoggerFactory>();



                services.AddJobMan(opt =>
                {
                    opt.CleanJobsScheduleCron = null;
                    opt.WorkPoolFactory = new SynchronizedWorkPoolFactory();
                    opt.DefaultPolicy.FailureRetryCount = 4;
                    opt.DefaultPolicy.FailureRetryWaitTimes = new int[] { 100 };
                    opt.UseStorage<InMemoryStorage>(null); //Different way to add storage
                    opt.JobExecutionFilter.Add(filter);

                });

                ServiceProvider sProvider = services.BuildServiceProvider();

                WorkServer workServer = sProvider.GetRequiredService<IWorkServer>() as WorkServer;
                Assert.NotNull(workServer);
                Assert.Single(workServer.Pools);

                workServer.StartAsync(CancellationToken.None).Wait();

                workServer.Enqueue(() => SampleFailureAction1("Exception1"));

                Thread.Sleep(5000);

                Assert.Equal(4, filter.LastWorkItem.Definition.RetryCount);

                workServer.StopAsync(CancellationToken.None).Wait();

            }
            finally
            {

            }

        }
    }
#pragma warning restore xUnit1013 // Public method should be marked as test
}
