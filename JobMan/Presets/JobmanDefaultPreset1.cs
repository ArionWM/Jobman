using JobMan.Storage.MemoryStorage;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public static class JobmanDefaultPreset1
    {
        public const string POOL_Default = WorkPoolOptions.POOL_DEFAULT;
        public const string POOL_Low = "Low";
        public const string POOL_Lowest = "Lowest";
        public const string POOL_Indexing = "Indexing";
        public const string POOL_Signals = "Signals";
        public const string POOL_Mail = "Mail";
        public const string POOL_Notification = "Notification";
        public const string POOL_LowestInMemory = "LowestInMemory";

        public static IWorkServerOptions AddPresetPoolOptions(IWorkServerOptions opt, int defaultThreadCount)
        {
            var defaultPoolOptions = opt[POOL_Default];
            defaultPoolOptions.ThreadCount = defaultThreadCount;

            opt.AddPool(POOL_Low.ToString(),
                popt =>
                {
                    popt.ThreadCount = defaultThreadCount / 2;
                    popt.Priority = ThreadPriority.BelowNormal;
                });

            opt.AddPool(POOL_Lowest.ToString(),
                popt =>
                {
                    popt.ThreadCount = 1;
                    popt.Priority = ThreadPriority.Lowest;
                });

            opt.AddPool(POOL_Indexing.ToString(),
                popt =>
                {
                    popt.ThreadCount = 2;
                    popt.IdlePeriod = new TimeSpan(0, 0, 2, 30, 0);
                    popt.Priority = ThreadPriority.Lowest;
                });

            opt.AddPool(POOL_Signals.ToString(),
                popt =>
                {
                    popt.ThreadCount = 8;
                    popt.IdlePeriod = new TimeSpan(0, 0, 0, 0, 50);
                    //popt.PreProcessBufferLenght = 1000;
                    popt.Priority = ThreadPriority.AboveNormal;
                    popt.UseInMemoryStorage();
                });

            opt.AddPool(POOL_Mail.ToString(),
               popt =>
               {
                   popt.ThreadCount = 2;
                   popt.IdlePeriod = new TimeSpan(0, 0, 2, 30, 0);
                   popt.Priority = ThreadPriority.BelowNormal;
               });

            opt.AddPool(POOL_Notification.ToString(),
              popt =>
              {
                  popt.ThreadCount = 2;
                  popt.IdlePeriod = new TimeSpan(0, 0, 0, 15, 0);
                  popt.Priority = ThreadPriority.BelowNormal;
                  popt.UseInMemoryStorage();
              });

            opt.AddPool(POOL_LowestInMemory.ToString(),
              popt =>
              {
                  popt.ThreadCount = 2;
                  popt.IdlePeriod = new TimeSpan(0, 0, 0, 15, 0);
                  popt.Priority = ThreadPriority.BelowNormal;
                  popt.ShowInUi = false;
                  popt.UseInMemoryStorage();
              });


            return opt;
        }
    }
}
