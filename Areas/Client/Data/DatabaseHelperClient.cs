using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperClient
    {
        private readonly AppDbContextClient _context;

        // Sử dụng DI để inject AppDbContextClient vào
        public DatabaseHelperClient(AppDbContextClient context)
        {
            _context = context;
        }

        public async Task<CuaHangModel_cuaHang> GetCuaHangByUserId(int userId)
        {
            return await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
        }
    }
}
