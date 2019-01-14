using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JaegerNetCoreFirst.App_Data
{
    [Route("api")]
    public class BController : Controller
    {
        [HttpGet, Route("GetValues", Name = "GetValues")]
        public async Task<string[]> GetValues()
        {
                var service = new AService();
                var result = await service.GetValues();
                return result;
        }
        
        [HttpGet, Route("HealthCheck", Name = "HealthCheck")]
        public string HealthCheck()
        {
            return "health check service A";
        }
    }
}
