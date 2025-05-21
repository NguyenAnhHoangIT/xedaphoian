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
using Microsoft.AspNetCore.Hosting;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class HomeController : Controller
    {
        private readonly AppDbContextClient _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(AppDbContextClient context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
                .Where(ch => ch.TrangThaiCuaHang == "true") // Lọc theo trạng thái cửa hàng
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
        public async Task<IActionResult> ThongTinTaiKhoan()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var user = await _context.TaiKhoan
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return View(user); // Truyền user sang View
        }

        [Route("Client")]
        [Route("Client/ThongTinTaiKhoan")]
        [HttpPost]
        public async Task<IActionResult> ThongTinTaiKhoan(UserModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Cập nhật thông tin từ form
            user.Ho = model.Ho;
            user.Ten = model.Ten;
            user.SoDienThoai = model.SoDienThoai;
            user.MatKhau = model.MatKhau; // Bạn nên hash mật khẩu nếu cần

            await _context.SaveChangesAsync();

            ViewBag.ThongBao = "Cập nhật thành công!";
            return View(user); // Trả lại view với thông tin đã cập nhật
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


        [Route("Client")]
        [Route("Client/DangKyCuaHang")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKyCuaHang(CuaHangModel_Client cuaHang)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Không lấy được ID => về đăng nhập
                TempData["error"] = "Bạn cần đăng nhập để đăng ký cửa hàng.";
                return RedirectToAction("Login", "Account");
            }
            cuaHang.IdTaiKhoan = int.Parse(userId);
            if (ModelState.IsValid)
            {
                var tenCuaHang = await _context.CuaHang.FirstOrDefaultAsync(p => p.TenCuaHang == cuaHang.TenCuaHang);
                if (tenCuaHang != null)
                {
                    ModelState.AddModelError("", "Tên cửa hàng đã tồn tại");
                    return View(cuaHang);
                }
                if (cuaHang.IMageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/cuaHang");
                    string imageName = Guid.NewGuid().ToString() + "_" + cuaHang.IMageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await cuaHang.IMageUpload.CopyToAsync(fs);
                    fs.Close();
                    cuaHang.HinhAnh = imageName;
                    cuaHang.TrangThaiCuaHang = "Pending";

                }
                _context.Add(cuaHang);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");

            }
            else
            {
                TempData["error"] = "Dữ liệu nhập vào chưa phù hợp";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }


            return View(cuaHang);
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
