using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace JaegerNetCoreFirst.App_Data
{
    [Route("api")]
    public class BController : Controller
    {
        // GET api/values
        [HttpGet, Route("GetValues", Name = "GetValues")]
        public async Task<string[]> GetValues()
        {
                var service = new AService();
                var result = await service.GetValues();
                return result;
        }
    }
}
