using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace JaegerNetCoreThird.App_Data
{
    public class CService
    {
        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'lal' ";

        public async Task<string> GetValues()
        {
            var connectionString = ConsulSettings.ConnectionString;
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var command = new CommandDefinition(GetValuesQuery);
                return (await db.QueryAsync<string>(command)).First();
            }
        }
    }
}