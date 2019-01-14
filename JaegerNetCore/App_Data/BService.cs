using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Dapper;

namespace JaegerNetCoreSecond.App_Data
{
    public class BService
    {
        private const string NameNextService = "Third";
        private readonly WebClient _webClient = new WebClient();
        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'lol' ";

        public async Task<string[]> GetValues()
        {
            var url = ConsulSettings.Url ?? GetUrl();
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

        public string GetUrl()
        {
            using (var consulClient = new ConsulClient())
            {
                var services = consulClient.Agent.Services().GetAwaiter().GetResult().Response;
                var address = services[NameNextService].Address;
                var port = services[NameNextService].Port;
                return ConsulSettings.Url = address + ":" + port + "/api/GetValues";
            }
        }
    }
}