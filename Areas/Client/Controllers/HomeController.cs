using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Security.Claims;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class HomeController : Controller
    {
        private readonly AppDbContextClient _context;

        public HomeController(AppDbContextClient context)
        {
            _context = context;
        }

        [Route("Client")]
        [Route("Client/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ThongTinTaiKhoan")]
        public IActionResult ThongTinTaiKhoan()
        {
            return View();
        }


        [Route("Client")]
        [Route("Client/DangKyCuaHang")]
        public async Task<IActionResult> DangKyCuaHang()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);

            var existingCuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(c => c.idTaiKhoan == userId);

            if (existingCuaHang != null)
            {
                return RedirectToAction("ThongTinCuaHang", "CuaHang", new { area = "Client" });
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Client/DangKyCuaHang")]
        public async Task<IActionResult> DangKyCuaHang(IFormCollection form, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);

            // Kiểm tra xem user đã có cửa hàng chưa
            var existingCuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(c => c.idTaiKhoan == userId);

            if (existingCuaHang != null)
            {
                return RedirectToAction("ThongTinCuaHang", "CuaHang", new { area = "Client" });
            }

            string fileName = null;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                // Giữ nguyên tên ảnh nhưng thêm một GUID vào để đảm bảo tính duy nhất
                var fileExtension = Path.GetExtension(hinhAnh.FileName);
                fileName = Path.GetFileNameWithoutExtension(hinhAnh.FileName) + "-" + Guid.NewGuid() + fileExtension;

                // Đường dẫn lưu vào thư mục /images/cuahang
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cuahang");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var path = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(stream);
                }
            }

            var cuaHang = new CuaHangModel_cuaHang
            {
                idTaiKhoan = userId,
                tenCuaHang = form["tenCuaHang"],
                diaChi = form["diaChi"],
                soDienThoai = form["soDienThoai"],
                gmail = form["gmail"],
                gioiThieu = form["gioiThieu"],
                hinhAnh = fileName != null ? "/images/cuahang/" + fileName : null, // Đảm bảo đường dẫn là "/images/cuahang/{fileName}"
                trangThaiCuaHang = "False" // chưa duyệt
            };

            _context.CuaHang.Add(cuaHang);
            await _context.SaveChangesAsync();

            // Cập nhật role thành Shop
            var identity = (ClaimsIdentity)User.Identity;
            var oldRoleClaim = identity.FindFirst(ClaimTypes.Role);
            if (oldRoleClaim != null) identity.RemoveClaim(oldRoleClaim);
            identity.AddClaim(new Claim(ClaimTypes.Role, "Shop"));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["ThongBao"] = "Cửa hàng của bạn đang chờ admin duyệt.";
            return RedirectToAction("Index", "Home", new { area = "Client" });
        }

        [Authorize]
        [Route("Client/CuaHangRedirect")]
        public async Task<IActionResult> CuaHangRedirect()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);

            if (cuaHang != null)
            {
                if (cuaHang.trangThaiCuaHang != "True")
                {
                    TempData["ThongBao"] = "Cửa hàng của bạn đang chờ admin duyệt.";
                    return RedirectToAction("Index", "Home", new { area = "Client" });
                }

                return RedirectToAction("DanhSachXe", "CuaHang", new { area = "Client" });
            }

            return RedirectToAction("DangKyCuaHang", "Home", new { area = "Client" });
        }


        [Route("Client/DangXuat")]
        [Route("dangxuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
