using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JaegerNetCoreSecond.App_Data
{
    [Route("api")]
    public class BController : Controller
    {
        [HttpGet, Route("GetValues", Name = "GetValues")]
        public async Task<string[]> GetValues()
        {
            var service = new BService();
            var result = await service.GetValues();
            return result;
        }

        [HttpGet, Route("HealthCheck", Name = "HealthCheck")]
        public string HealthChecks()
        {
            return "health check service B";
        }
    }
}