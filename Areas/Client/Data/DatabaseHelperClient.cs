using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperClient
    {
        private readonly AppDbContextClient _context;
        private readonly string _connectionString;

        public DatabaseHelperClient(AppDbContextClient context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<CuaHangModel_Client> GetCuaHangByUserId(int userId)
        {
            return await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
        }
    }

}
