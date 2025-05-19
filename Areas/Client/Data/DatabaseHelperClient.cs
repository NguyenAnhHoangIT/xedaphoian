using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperClient
    {
        private readonly string _connectionString;
        private readonly AppDbContextClient _context;

        public DatabaseHelperClient(IConfiguration configuration)
        // Sử dụng DI để inject AppDbContextClient vào
        public DatabaseHelperClient(AppDbContextClient context)
        {
            _context = context;
        }

        public async Task<CuaHangModel_cuaHang> GetCuaHangByUserId(int userId)
        {
            return await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            _connectionString = configuration.GetConnectionString("Default");
        }
    }
}
