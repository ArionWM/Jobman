using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.TestHelpers
{
    internal class TestLoader
    {
        static Random _random;

        static TestLoader()
        {
            if (_random == null)
                _random = new Random(DateTime.Now.Millisecond);
        }

        public static void TestJob(int durationMs)
        {
            //if (_random.Next(100) % 20 == 0) //%4 possibility
            //    throw new Exception("Test exception");

            Thread.Sleep(durationMs);
        }

        public async void CreateLoad(int count, int maxDurationMs)
        {
            string[] poolNames = JobManGlobals.Server.Pools.Select(pool => pool.Name).ToArray();

            for (int i = 0; i < count; i++)
            {
                string poolName = poolNames[_random.Next(poolNames.Length)];
                JobManGlobals.Server.Enqueue(poolName, () => TestJob(_random.Next(2, maxDurationMs)));
            }

            await Task.CompletedTask;
        }
    }
}
