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
using ThueXeDapHoiAn.Areas.Admin.ViewModels;

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
                        .Average(),
                    DiaChi = ch.DiaChi
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
        public async Task<IActionResult> Search(
    string searchTerm,
    decimal? minPrice,
    decimal? maxPrice,
    string priceRange,
    string sortOrder)
        {
            var query = _context.Xe.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.TenXe.Contains(searchTerm) || p.GioiThieu.Contains(searchTerm));
            }

            // Xử lý lọc theo khoảng giá có sẵn
            if (!string.IsNullOrEmpty(priceRange) && priceRange != "custom")
            {
                if (priceRange.EndsWith("+"))
                {
                    // Ví dụ: 200000+
                    if (decimal.TryParse(priceRange.Replace("+", ""), out decimal min))
                    {
                        query = query.Where(p => p.GiaThueTheoGio >= min);
                    }
                }
                else if (priceRange.Contains('-'))
                {
                    var parts = priceRange.Split('-');
                    if (decimal.TryParse(parts[0], out decimal min) && decimal.TryParse(parts[1], out decimal max))
                    {
                        query = query.Where(p => p.GiaThueTheoGio >= min && p.GiaThueTheoGio <= max);
                    }
                }
            }

            // Xử lý lọc giá tuỳ chọn nếu người dùng chọn custom
            if (priceRange == "custom")
            {
                if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
                {
                    ViewData["Error"] = "Giá tối thiểu không được lớn hơn giá tối đa.";
                    ViewBag.Keyword = searchTerm;
                    return View(new List<XeModel_Client>());
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.GiaThueTheoGio >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.GiaThueTheoGio <= maxPrice.Value);
                }
            }

            // Xử lý sắp xếp theo giá
            if (sortOrder == "asc")
            {
                query = query.OrderBy(p => p.GiaThueTheoGio);
            }
            else if (sortOrder == "desc")
            {
                query = query.OrderByDescending(p => p.GiaThueTheoGio);
            }

            var xe = await query.ToListAsync();

            // Gửi lại giá trị để ViewComponent nhớ lựa chọn
            ViewBag.Keyword = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SelectedRange = priceRange;
            ViewBag.SortOrder = sortOrder;

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTinTaiKhoan(UserModel model, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Xử lý ảnh nếu có upload mới
                if (hinhAnh != null && hinhAnh.Length > 0)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images/nguoiDung");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                    string filePath = Path.Combine(uploadDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await hinhAnh.CopyToAsync(fs);
                    }

                    user.HinhAnh = imageName; // Gán ảnh mới
                }

                // Cập nhật các thông tin khác
                user.Ho = model.Ho;
                user.Ten = model.Ten;
                user.SoDienThoai = model.SoDienThoai;
                user.MatKhau = model.MatKhau; // Cân nhắc mã hóa nếu cần

                await _context.SaveChangesAsync();

                // Gán ViewBag cho ảnh và tên người dùng
                ViewBag.ThongBao = "Cập nhật thông tin thành công!";
                ViewBag.TenNguoiDung = user.Ho + " " + user.Ten;
                ViewBag.AvatarNguoiDung = string.IsNullOrEmpty(user.HinhAnh)
                    ? Url.Content("~/images/avatar-default.jpg")
                    : Url.Content("~/images/nguoiDung/" + user.HinhAnh);

                return View(user);
            }
            else
            {
                TempData["error"] = "Dữ liệu nhập vào không hợp lệ.";
                List<string> errors = new List<string>();
                foreach (var state in ModelState.Values)
                {
                    foreach (var error in state.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
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

        [Route("Client/AllCuaHang")]
        public async Task<IActionResult> AllCuaHang()
        {
            // Lấy danh sách cửa hàng
            var allCuaHang = await _context.CuaHang.Where(c=>c.TrangThaiCuaHang.Equals("True")).ToListAsync();

            // Lấy tất cả đánh giá liên quan đến các cửa hàng đó (dùng để tính điểm trung bình)
            var allDanhGia = await _context.DanhGia
                .Include(dg => dg.DonThue)
                .Where(dg => allCuaHang.Select(ch => ch.IdCuaHang).Contains(dg.DonThue.IdCuaHang))
                .ToListAsync();

            // Tạo list viewmodel cửa hàng kèm điểm trung bình đánh giá
            var model = allCuaHang.Select(ch =>
            {
                var danhGiaCuaHang = allDanhGia.Where(dg => dg.DonThue.IdCuaHang == ch.IdCuaHang);
                double diemTrungBinh = danhGiaCuaHang.Any() ? Math.Round(danhGiaCuaHang.Average(dg => dg.DiemDanhGia), 1) : 0;

                return new CuaHangViewModel2
                {
                    IdCuaHang = ch.IdCuaHang,
                    TenCuaHang = ch.TenCuaHang,
                    MoTa = ch.GioiThieu,
                    HinhAnh = ch.HinhAnh,
                    DiemTrungBinh = diemTrungBinh,
                    DiaChi = ch.DiaChi
                };
            }).ToList();

            return View(model);
        }


    }
}
