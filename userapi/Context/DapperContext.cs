using Microsoft.Data.SqlClient;
using System.Data;

namespace UserApi.Context
{
    public class DapperContext
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public DapperContext (IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    }
}
