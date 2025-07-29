using Microsoft.AspNetCore.Mvc;

namespace JobMan.Sample01.Controllers
{
    public class SampleJobmanMethodsController : Controller
    {

        static readonly string[] sampleWorkpoolNames = new string[] { "Default", "Low", "Lowest", "Indexing", "Signals" };

        public IWorkServer WorkSrv { get; }

        public SampleJobmanMethodsController(IWorkServer workSrv)
        {
            WorkSrv = workSrv;
        }





        public IActionResult AddJob1()
        {
            Random random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 100; i++)
                this.WorkSrv.Enqueue(() => JobmanSampleMethodContainer.Job1("parameter1", 2));

            return Redirect("/jobman");
        }

        public IActionResult AddJob2()
        {
            Random random = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < 10000; i++)
            {
                int itemIndex = random.Next(0, sampleWorkpoolNames.Length);
                string poolName = sampleWorkpoolNames[itemIndex];


                this.WorkSrv.Enqueue(poolName, () => JobmanSampleMethodContainer.Job1("parameter2", 3));
            }

            return Redirect("/jobman");
        }


    }
}
