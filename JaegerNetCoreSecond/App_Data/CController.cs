using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace JaegerNetCoreThird.App_Data
{
    [Route("api")]
    public class CController : Controller
    {
        // GET api/values
        [HttpGet, Route("GetValues", Name = "GetValues")]
        public async Task<string> GetValue()
        {
                var service = new CService();
                var result = await service.GetValues();
                return result;
        }
    }
}
