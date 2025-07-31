using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jobman.UI.AspNetCore;

public static class ServiceExtensions
{
    //public static void AddJobManUi(this IServiceCollection services)
    //{
    //    //Nothing yet ..
    //}


    //IApplicationBuilder + IEndpointRouteBuilder ?
    public static void UseJobManUi(this WebApplication app)
    {
        app.UseStaticFiles();

        app.MapControllerRoute(
            name: "Areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}");

        //_ = app.Services.GetRequiredService<IWorkServer>().StartAsync();
    }
}
