using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperClient
    {
        private readonly string _connectionString;

        public DatabaseHelperClient(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }
    }
}
