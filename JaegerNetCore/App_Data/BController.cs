using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace JaegerNetCoreSecond.App_Data
{
    [Route("api")]
    public class BController : Controller
        {
            // GET api/values
            [HttpGet, Route("GetValues", Name = "GetValues")]
            public async Task<string[]> GetValues()
            {
                    var service = new BService();
                    var result = await service.GetValues();
                    return result;
            }
        }
}
