using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Security.Claims;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Areas.Client.Models;

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
            var dsXe = _context.Xe.ToList();

            var danhGiaJoin = (from dg in _context.DanhGia
                               join dt in _context.DonThue on dg.IdDonThue equals dt.IdDonThue
                               select new { dg.DiemDanhGia, dt.IdCuaHang }).ToList();

            var dsCuaHang = _context.CuaHang
                .ToList()
                .Select(ch => new CuaHang_Index_ViewModel_Client
                {
                    IdCuaHang = ch.IdCuaHang,
                    TenCuaHang = ch.TenCuaHang,
                    MoTa = ch.GioiThieu,
                    HinhAnh = ch.HinhAnh,
                    DiemTrungBinh = danhGiaJoin
                        .Where(x => x.IdCuaHang == ch.IdCuaHang)
                        .Select(x => x.DiemDanhGia)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();


            var viewModel = new CuaHang_Xe_ViewModel_Client
            {
                DanhSachXe = dsXe,
                DanhSachCuaHang = dsCuaHang
            };

            return View(viewModel);
        }


        [HttpGet("Client/GetThongBao")]
        public IActionResult GetThongBao()
        {
            var idTaiKhoan = int.Parse(User.FindFirstValue("idTaiKhoan"));
            var thongBaoList = _context.ThongBao
            .Where(tb => tb.IdTaiKhoanNhan == idTaiKhoan) // sửa thành idTaiKhoanNhan
            .OrderByDescending(tb => tb.ThoiGianTao)
            .Take(5)
            .Select(tb => new
            {
                tb.TieuDe,
                tb.NoiDung,
                ThoiGian = tb.ThoiGianTao.ToString("dd/MM/yyyy HH:mm")
            })
            .ToList();


            return Json(thongBaoList);
        }


        [Route("Client")]
        [Route("Client/Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var xe = await _context.Xe.Where(p => p.TenXe.Contains(searchTerm) || p.GioiThieu.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(xe);

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
                .FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);

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
                .FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);

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

            var cuaHang = new CuaHangModel_Client
            {
                IdTaiKhoan = userId,
                TenCuaHang = form["tenCuaHang"],
                DiaChi = form["diaChi"],
                SoDienThoai = form["soDienThoai"],
                Gmail = form["gmail"],
                GioiThieu = form["gioiThieu"],
                HinhAnh = fileName != null ? "/images/cuahang/" + fileName : null, // Đảm bảo đường dẫn là "/images/cuahang/{fileName}"
                TrangThaiCuaHang = "False" // chưa duyệt
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);

            if (cuaHang != null)
            {
                if (cuaHang.TrangThaiCuaHang == "True")
                {
                    return RedirectToAction("DanhSachXe", "CuaHang", new { area = "Client" });
                }
                else
                {
                    return Redirect("/Client/ChoDuyet");
                }
            }

            // Nếu chưa có cửa hàng thì về trang đăng ký cửa hàng
            return Redirect("/Client/DangKyCuaHang");
        }


        [Route("Client/DangXuat")]
        [Route("dangxuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Route("Client/ChoDuyet")]
        public IActionResult CuaHang_ChoDuyet()
        {
            return View();
        }

    }
}
