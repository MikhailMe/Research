using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
                return dbRes.ToArray();
            }
        }

    }
}
