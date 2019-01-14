using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace JaegerNetCoreThird.App_Data
{
    [Route("api")]
    public class CController : Controller
    {
        [HttpGet, Route("GetValues", Name = "GetValues")]
        public async Task<string> GetValue()
        {
            var service = new CService();
            var result = await service.GetValues();
            return result;
        }

        [HttpGet, Route("HealthCheck", Name = "HealthCheck")]
        public string HealthCheck()
        {
            return "health check service C";
        }
    }
}