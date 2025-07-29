using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobMan.AspNetCore.Ui.Areas.JobMan.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Area("JobMan")]
    [ApiController]
    
    public class MetricsController : ControllerBase
    {
        private readonly IWorkServer workServer;

        public MetricsController(IWorkServer workServer)
        {
            this.workServer = workServer;
        }

        [Route("jobman/metrics/server")]
        [HttpGet]
        public WorkServerMetrics Server()
        {
            //JsonSerializerOptions.Default
            WorkServerMetrics metrics = workServer.Metrics;
            return metrics;
        }
    }
}
