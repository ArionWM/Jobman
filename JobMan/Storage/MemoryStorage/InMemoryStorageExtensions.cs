using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Storage.MemoryStorage
{
    public static class InMemoryStorageExtensions
    {
        public static IWorkPoolOptions UseInMemoryStorage(this IWorkPoolOptions options)
        {
            options.UseStorage<InMemoryStorage>(null);

            return options;
        }

        public static IWorkServerOptions UseInMemoryStorage(this IWorkServerOptions options)
        {
            options.UseStorage<InMemoryStorage>(null);

            return options;
        }
    }
}
