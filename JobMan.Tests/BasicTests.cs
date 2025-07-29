using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using JobMan.Tests.Mock;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using JobMan.Storage.MemoryStorage;
using JobMan.Factories;

namespace JobMan.Tests
{
#pragma warning disable xUnit1013 // Public method should be marked as test

    public class BasicTests
    {
        //[ThreadStatic]
        static ConcurrentBag<string> _invokedMethods;

        public static ConcurrentBag<string> InvokedMethods => _invokedMethods ?? (_invokedMethods = new ConcurrentBag<string>());


        public static void SampleAction1()
        {
            InvokedMethods.Add(nameof(SampleAction1));
        }


        public static void SampleAction2(string myArg0, int myArg1)

        {
            InvokedMethods.Add(nameof(SampleAction2));
        }

        public static void SampleAction3()
        {
            InvokedMethods.Add(nameof(SampleAction3));
        }


        [Fact]
        public void WorkerSimpleProcessCheck()
        {
            int invokeCount = 0;
            int failCount = 0;

            Expression<Action> expression = () => SampleAction2("valueSm", 2);
            MockWorkPool mockWorkPool = new MockWorkPool();

            mockWorkPool.TestGetWorkItemOrWaitCallBack = () =>
            {
                Interlocked.Increment(ref invokeCount);

                return null;
            };

            mockWorkPool.FailureCallBack += (worker) =>
            {
                Interlocked.Increment(ref failCount);
            };

            Worker worker = new Worker(mockWorkPool);

            worker.Start();

            Thread.Sleep(1000);

            Assert.True(invokeCount > 0);
            Assert.True(failCount == 0);
        }


        [Fact]
        public void BasicRun()
        {
            InvokedMethods.Clear();
            var services = new ServiceCollection();

            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            services.AddJobMan(opt =>
            {
                opt
                    //.UseSqlServerStorage("abc");
                    .AddPool("Pool1",
                        opt =>
                        {
                            opt.ThreadCount = 3;
                            opt.Priority = ThreadPriority.Lowest;
                        });

                opt.UseInMemoryStorage(); //Set all pools to use in memory storage
            });

            ServiceProvider sProvider = services.BuildServiceProvider();

            WorkServer workServer = sProvider.GetRequiredService<IWorkServer>() as WorkServer;
            Assert.NotNull(workServer);
            Assert.Equal(2, workServer.Pools.Length);

            workServer.StartAsync(CancellationToken.None).Wait();

            workServer.Enqueue(() => SampleAction1()); //Add to default pool
            workServer.Enqueue(() => SampleAction2("MyValue1", 2));  //Add to default pool
            workServer.Enqueue("Pool1", () => SampleAction3()); //Add to pool1

            Thread.Sleep(2000);

            Assert.Equal(3, InvokedMethods.Count);
            Assert.Contains(nameof(SampleAction1), InvokedMethods);
            Assert.Contains(nameof(SampleAction2), InvokedMethods);

            workServer.StopAsync(CancellationToken.None).Wait();
        }

        [Fact]
        public void BasicScheduledRun()
        {
            InvokedMethods.Clear();

            var services = new ServiceCollection();
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            services.AddJobMan(opt =>
            {
                opt.CleanJobsScheduleCron = null;
                opt.UseStorage<InMemoryStorage>(null); //Different way to add storage
            });

            ServiceProvider sProvider = services.BuildServiceProvider();

            WorkServer workServer = sProvider.GetRequiredService<IWorkServer>() as WorkServer;
            Assert.NotNull(workServer);
            Assert.Single(workServer.Pools);

            workServer.StartAsync(CancellationToken.None).Wait();

            workServer.Schedule(() => SampleAction1(), "0 0 * * *");

            Thread.Sleep(1000);

            //Let's move our clocks forward one day
            JobManGlobals.Time = new TestTimeResolver(DateTime.Now.AddDays(1));

            //Process work items
            Thread.Sleep(5000);

            Assert.Equal(1, InvokedMethods.Count);
            Assert.Contains(nameof(SampleAction1), InvokedMethods);

            InMemoryStorage memoryStorage = DefaultStorageFactory.Get<InMemoryStorage>(null);   

            IWorkItemDefinition scheduledItem = memoryStorage.Schedules.Values.First();
            Assert.Equal(WorkItemStatus.WaitingProcess, scheduledItem.Status);

            workServer.StopAsync(CancellationToken.None).Wait();
        }

    }

#pragma warning restore xUnit1013 // Public method should be marked as test
}
