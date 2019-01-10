using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Dapper;

namespace JaegerNetCoreSecond.App_Data
{
    public class BService
    {
        private string _connectionString = @"";
        private readonly WebClient _webClient = new WebClient();
        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'lol' ";

        public async Task<string[]> GetValues()
        {
            var url = "http://localhost:56509/api/GetValues";
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var command = new CommandDefinition(GetValuesQuery);
                var dbRes = (await db.QueryAsync<string>(command)).ToList();
                var res = _webClient.DownloadString(url);
                dbRes.Add(res);
                HelloConsul().GetAwaiter().GetResult();
                return dbRes.ToArray();
            }
        }

        public static async Task<string> HelloConsul()
        {
            using (var client = new ConsulClient())
            {
                var putPair = new KVPair("hello")
                {
                    Value = Encoding.UTF8.GetBytes("Hello Consul")
                };

                var putAttempt = await client.KV.Put(putPair);

                if (putAttempt.Response)
                {
                    var getPair = await client.KV.Get("hello");
                    return Encoding.UTF8.GetString(getPair.Response.Value, 0,
                        getPair.Response.Value.Length);
                }
                return "";
            }
        }

    }
}
