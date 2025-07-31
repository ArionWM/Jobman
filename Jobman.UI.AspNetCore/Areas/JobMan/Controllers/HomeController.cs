using Microsoft.AspNetCore.Mvc;
using JobMan.TestHelpers;
using JobMan;

namespace Jobman.UI.AspNetCore.Areas.JobMan.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Area("JobMan")]
public class HomeController : Controller
{
    private readonly IWorkServer workServer;

    public HomeController(IWorkServer workServer)
    {
        this.workServer = workServer;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [HttpGet]
    public IActionResult AddLoadTest()
    {
        TestLoader testLoader = new TestLoader();
        Task.Run(() =>
        {
            testLoader.CreateLoad(10000, 2000);
        });

        return Ok();
    }
}
