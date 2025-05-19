using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Models;

namespace ThueXeDapHoiAn.Areas.Client.ViewComponents
{
    public class LoaiXeViewComponent : ViewComponent
    {
        private readonly AppDbContextClient _context;
        public LoaiXeViewComponent(AppDbContextClient context)
        {
            _context = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy idCuaHang từ query string (?id=2)
            if (!int.TryParse(HttpContext.Request.Query["id"], out int idCuaHang))
            {
                // Nếu không có idCuaHang hợp lệ, trả về danh sách rỗng
                return View(new List<LoaiXeModel_Client>());
            }

            // Lấy danh sách loại xe theo idCuaHang
            var loaiXeList = await _context.LoaiXe
                .Where(lx => lx.IdCuaHang == idCuaHang)
                .ToListAsync();

            return View(loaiXeList);
        }
    }
}
