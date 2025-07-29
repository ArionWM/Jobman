using JobMan.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public static class ServiceExtensions
    {
        public static void AddJobMan(this IServiceCollection services, Action<IWorkServerOptions> options)
        {
            //TODO: Use Options pattern
            services.AddSingleton<ITimeResolver, DefaultTimeResolver>();
            services.AddSingleton<IWorkServer>(sp =>
            {

                JobManGlobals.LoggerFactory = sp.GetService<ILoggerFactory>();
                JobManGlobals.Time = sp.GetService<ITimeResolver>();

                IWorkServerOptions wsOptions = new WorkServerOptions();

                options.Invoke(wsOptions);

                if (!wsOptions.PoolOptions.Any(po => po.Name == WorkPoolOptions.POOL_DEFAULT))
                    wsOptions.AddPool(WorkPoolOptions.POOL_DEFAULT, opt =>
                    {
                        opt.StorageOptions = DefaultStorageFactory.DefaultOptions;
                    });

                WorkServer server = new WorkServer(wsOptions, sp.GetService<ILogger<WorkServer>>());
                JobManGlobals.Server = server;

                return server;
            });

            services.AddHostedService<IWorkServer>(sp => sp.GetRequiredService<IWorkServer>());
        }


    }
}
