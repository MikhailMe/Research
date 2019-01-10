using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using JaegerNetCoreSecond.App_Data;
using Newtonsoft.Json.Linq;

namespace JaegerNetCoreFirst.App_Data
{
    public class AService
    {
        private readonly WebClient _webClient = new WebClient();
        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'lul' ";

        public async Task<string[]> GetValues()
        {
            var url = ConsulSettings.Url ?? await GetUrl();
            var connectionString = ConsulSettings.ConnectionString;
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var command = new CommandDefinition(GetValuesQuery);
                var dbRes = (await db.QueryAsync<string>(command)).ToList();
                var res = _webClient.DownloadString(url);
                dbRes.Add(res);
                return dbRes.ToArray();
            }
        }

        public async Task<string> GetUrl()
        {
            var serviceSettings = await new HttpClient().GetStringAsync("http://localhost:8500/v1/catalog/service/Second");
            var serviceSettingsJson = JObject.Parse(Utils.GetJson(serviceSettings));
            var servicePort = (string)serviceSettingsJson["ServicePort"];
            return ConsulSettings.Url = "http://localhost:" + servicePort + "/api/GetValues";
        }
    }
}
