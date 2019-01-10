using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace JaegerNetCoreThird.App_Data
{
    public class CService
    {
        private string _connectionString = @"";

        private const string GetValuesQuery = @"SELECT name FROM tableTest where name = 'lal' ";

        public async Task<string> GetValues()
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var command = new CommandDefinition(GetValuesQuery);
                return (await db.QueryAsync<string>(command)).First();
            }
        }

    }
}
