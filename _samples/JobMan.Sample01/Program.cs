using JobMan;
using Jobman.UI.AspNetCore;
using JobMan.Storage.MemoryStorage;
using Serilog;

namespace JobMan.Sample01
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            /*
             Create any logger
             */

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();


            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);


            /*
             Sample jobman service creation
             */

            builder.Services.AddJobMan(opt =>
            {
                int defaultThreadCount = 8;

                opt.AddPool(WorkPoolOptions.POOL_DEFAULT,
                    popt =>
                    {
                        popt.ThreadCount = defaultThreadCount ;
                        popt.Priority = ThreadPriority.Normal;
                    });

                opt.AddPool("Low",
                    popt =>
                    {
                        popt.ThreadCount = defaultThreadCount / 2;
                        popt.Priority = ThreadPriority.BelowNormal;
                    });

                opt.AddPool("Lowest",
                    popt =>
                    {
                        popt.ThreadCount = 4;
                        popt.Priority = ThreadPriority.Lowest;
                    });

                opt.AddPool("Indexing",
                    popt =>
                    {
                        popt.ThreadCount = 1;
                        popt.Priority = ThreadPriority.BelowNormal;
                    });

                opt.AddPool("Signals",
                    popt =>
                    {
                        popt.ThreadCount = 4;
                        popt.Priority = ThreadPriority.Normal;
                    });

                opt.AddPool("Mailing",
                    popt =>
                    {
                        popt.ThreadCount = 2;
                        popt.Priority = ThreadPriority.BelowNormal;
                    });

                //if use SQL server storage change lines
                //opt.UseInMemoryStorage();
                //opt.UseSqlServerStorage("Data Source=.\\SQLEXPRESS;Initial Catalog=silJobmanExam;User Id=utest;Password=h354Msd782A;MultipleActiveResultSets=true;Pooling=true;Min Pool Size=10; Max Pool Size=500;TrustServerCertificate=True");
                opt.UsePostgreSqlStorage("User ID=tuser1;Password=h4*3v34MU;Host=192.168.1.236;Port=5432;Database=vectortest02");
            });


            builder.Services.AddRazorPages();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //builder.Services.AddJobManUi(); // Add JobMan UI for services

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }


            app.UseRouting();

            app.UseAuthorization();

            
            app.UseJobManUi(); // Add JobMan UI for application ('/jobman')'

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            


            app.Run();
        }
    }
}
