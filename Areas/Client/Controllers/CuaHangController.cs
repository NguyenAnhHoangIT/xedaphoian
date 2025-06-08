using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Shop")]
    public class CuaHangController : Controller
    {
        private readonly AppDbContextClient _context;
        private readonly DatabaseHelperClient _dbHelper;

        public CuaHangController(AppDbContextClient context, DatabaseHelperClient dbHelper)
        {
            _context = context;
            _dbHelper = dbHelper;
        }
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThongTinCuaHang")]
        public async Task<IActionResult> ThongTinCuaHang()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);

            if (cuaHang == null)
            {
                return RedirectToAction("DangKyCuaHang", "Home", new { area = "Client" });
            }

            return View(cuaHang); // Truyền model sang view
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdStr))
            {
                int userId = int.Parse(userIdStr);
                var cuaHang = _context.CuaHang.FirstOrDefault(c => c.IdTaiKhoan == userId);
                if (cuaHang != null)
                {
                    ViewBag.TenCuaHang = cuaHang.TenCuaHang;
                    ViewBag.AvatarCuaHang = string.IsNullOrEmpty(cuaHang.HinhAnh)
                        ? "/images/avatar-default.jpg"
                        : (cuaHang.HinhAnh.StartsWith("/images/") ? cuaHang.HinhAnh : "/images/cuahang/" + cuaHang.HinhAnh);
                }
            }
            base.OnActionExecuting(context);
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThongTinCuaHang")]
        public async Task<IActionResult> CapNhatThongTin(CuaHangModel_Client model, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Xử lý ảnh đại diện
            string fileName = cuaHang.HinhAnh;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cuahang", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(stream);
                }
            }

            // Cập nhật các thông tin
            cuaHang.TenCuaHang = model.TenCuaHang;
            cuaHang.DiaChi = model.DiaChi;
            cuaHang.SoDienThoai = model.SoDienThoai;
            cuaHang.Gmail = model.Gmail;
            cuaHang.GioiThieu = model.GioiThieu;
            cuaHang.HinhAnh = fileName;

            _context.CuaHang.Update(cuaHang);
            await _context.SaveChangesAsync();

            return RedirectToAction("ThongTinCuaHang");
        }



        [Route("Client")]
        [Route("Client/Shop/DanhSachXe")]
        public async Task<IActionResult> DanhSachXe()
        {
            // Lấy ID cửa hàng từ người dùng hiện tại
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Lấy cửa hàng tương ứng với người dùng
            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Lấy danh sách xe của cửa hàng này
            var danhSachXe = await _context.Xe
                .Include(x => x.LoaiXe) // Bao gồm thông tin loại xe (LoạiXe)
                .Where(x => x.IdCuaHang == cuaHang.IdCuaHang) // Chỉ lấy xe của cửa hàng hiện tại
                .ToListAsync();

            // Trả về view và truyền danh sách xe vào
            return View(danhSachXe);
        }

        [Route("Client")]
        [Route("Client/Shop/ThemXe")]
        public async Task<IActionResult> ThemXe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Lọc loại xe theo idCuaHang của shop hiện tại
            ViewBag.LoaiXeList = await _context.LoaiXe
                .Where(lx => lx.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThemXe")]
        public async Task<IActionResult> ThemXe(XeModel_Client model, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            string fileName = null;
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                // Tạo tên file duy nhất
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "xe", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(stream);
                }
            }

            var xe = new XeModel_Client
            {
                TenXe = model.TenXe,
                IdLoaiXe = model.IdLoaiXe,
                SoLuong = model.SoLuong,
                GiaThueTheoGio = model.GiaThueTheoGio,
                GiaThueTheoNgay = model.GiaThueTheoNgay,
                GioiThieu = model.GioiThieu,
                HinhAnh = fileName,
                IdCuaHang = cuaHang.IdCuaHang,
                TrangThaiLoaiXe = true
            };

            _context.Xe.Add(xe);
            await _context.SaveChangesAsync();

            return RedirectToAction("DanhSachXe");
        }


        [HttpGet]
        [Route("Client/Shop/XoaXe")]
        public async Task<IActionResult> XoaXe(int id)
        {
            // Kiểm tra xe có nằm trong ChiTietDonThue không
            var exists = await _context.ChiTietDonThue.AnyAsync(ct => ct.IdXe == id);
            if (exists)
            {
                TempData["ErrorMessage"] = "Không thể xóa xe này vì đã hoặc đang có đơn thuê liên quan!";
                return RedirectToAction("DanhSachXe");
            }

            var xe = await _context.Xe.FirstOrDefaultAsync(x => x.IdXe == id);
            if (xe == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy xe để xóa!";
                return RedirectToAction("DanhSachXe");
            }

            _context.Xe.Remove(xe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa xe thành công!";
            return RedirectToAction("DanhSachXe");
        }

        // ...existing code...
        [HttpGet]
        [Route("Client/Shop/SuaXe")]
        public async Task<IActionResult> SuaXe(int id)
        {
            var xe = await _context.Xe.Include(x => x.LoaiXe).FirstOrDefaultAsync(x => x.IdXe == id);
            if (xe == null) return NotFound();

            // Lấy idCuaHang từ xe hoặc từ user đăng nhập
            int idCuaHang = xe.IdCuaHang;

            // Lọc loại xe theo idCuaHang của shop hiện tại
            ViewBag.LoaiXeList = await _context.LoaiXe
                .Where(lx => lx.IdCuaHang == idCuaHang)
                .ToListAsync();

            return View(xe);
        }
        // ...existing code...

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/SuaXe")]
        public async Task<IActionResult> SuaXe(XeModel_Client model, IFormFile hinhAnh)
        {
            // Lấy tên ảnh cũ từ database
            string fileName = null;
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var getCmd = conn.CreateCommand();
                getCmd.CommandText = "SELECT hinhAnh FROM Xe WHERE idXe = @idXe";
                getCmd.Parameters.AddWithValue("@idXe", model.IdXe);
                var result = await getCmd.ExecuteScalarAsync();
                fileName = result != DBNull.Value ? (string)result : null;
            }

            // Nếu có upload ảnh mới thì lưu lại và cập nhật tên file
            if (hinhAnh != null && hinhAnh.Length > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnh.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "xe", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await hinhAnh.CopyToAsync(stream);
                }
            }

            // Update bằng ADO.NET để tránh lỗi trigger với OUTPUT
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            UPDATE Xe SET 
                tenXe = @tenXe,
                idLoaiXe = @idLoaiXe,
                soLuong = @soLuong,
                giaThueTheoGio = @giaThueTheoGio,
                giaThueTheoNgay = @giaThueTheoNgay,
                gioiThieu = @gioiThieu,
                hinhAnh = @hinhAnh
            WHERE idXe = @idXe";
                cmd.Parameters.AddWithValue("@tenXe", model.TenXe);
                cmd.Parameters.AddWithValue("@idLoaiXe", model.IdLoaiXe);
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@giaThueTheoGio", model.GiaThueTheoGio);
                cmd.Parameters.AddWithValue("@giaThueTheoNgay", model.GiaThueTheoNgay);
                cmd.Parameters.AddWithValue("@gioiThieu", (object?)model.GioiThieu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hinhAnh", (object?)fileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idXe", model.IdXe);
                await cmd.ExecuteNonQueryAsync();
            }

            TempData["SuccessMessage"] = "Cập nhật xe thành công!";
            return RedirectToAction("DanhSachXe");
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueChoDuyet")]
        public async Task<IActionResult> DonThueChoDuyet()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Truy vấn để lấy đơn thuê và chi tiết đơn thuê bao gồm thông tin xe
            var donThueChoDuyet = await _context.DonThue
                .Include(d => d.ChiTietDonThue)  // Bao gồm ChiTietDonThue
                    .ThenInclude(ct => ct.Xe)   // Bao gồm Xe
                    .ThenInclude(x => x.LoaiXe) // Bao gồm LoaiXe
                .Include(d => d.User)   // Bao gồm KhachHang
                .Include(d => d.KhuyenMai)
                .Where(d => d.TrangThaiDon == "Đang chờ duyệt" && d.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View(donThueChoDuyet); // Trả về View với danh sách đơn thuê
        }
        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/DuyetDonThue")]
        public async Task<IActionResult> DuyetDonThue(int id)
        {
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE DonThue SET trangThaiDon = N'Đang thuê' WHERE idDonThue = @id";
                cmd.Parameters.AddWithValue("@id", id);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return NotFound();
            }
            return RedirectToAction("DonThueDaDuyet");
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/HuyDonThue")]
        public async Task<IActionResult> HuyDonThue(int id)
        {
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE DonThue SET trangThaiDon = N'Đã huỷ' WHERE idDonThue = @id";
                cmd.Parameters.AddWithValue("@id", id);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return NotFound();
            }
            return RedirectToAction("DonThueDaHuy");
        }


        [Route("Client")]
        [Route("Client/Shop/DonThueDaDuyet")]
        public async Task<IActionResult> DonThueDaDuyet()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var trangThaiChoPhep = new[] { "Đã duyệt đơn", "Đang thuê", "Quá hạn" };

            var donThueDaDuyet = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.User)
                .Include(d => d.KhuyenMai)
                .Where(d => trangThaiChoPhep.Contains(d.TrangThaiDon) && d.IdCuaHang == cuaHang.IdCuaHang)

                .ToListAsync();

            return View(donThueDaDuyet);
        }


        [Route("Client")]
        [Route("Client/Shop/DonThueDaHuy")]
        public async Task<IActionResult> DonThueDaHuy()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var donThueDaHuy = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.User)
                .Include(d => d.KhuyenMai)
                .Where(d => d.TrangThaiDon == "Đã huỷ" && d.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View(donThueDaHuy);
        }
        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/HoanThanhDonThue")]
        public async Task<IActionResult> HoanThanhDonThue(int id)
        {
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE DonThue SET trangThaiDon = N'Hoàn thành' WHERE idDonThue = @id";
                cmd.Parameters.AddWithValue("@id", id);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return NotFound();
            }
            return RedirectToAction("DonThueHoanThanh");
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueHoanThanh")]
        public async Task<IActionResult> DonThueHoanThanh()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var donThueHoanThanh = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.User)
                .Include(d => d.KhuyenMai)
                .Where(d => d.TrangThaiDon == "Hoàn thành" && d.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View(donThueHoanThanh);
        }

        [Route("Client")]
        [Route("Client/Shop/DanhSachKhuyenMai")]
        public async Task<IActionResult> DanhSachKhuyenMai()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Lấy cửa hàng của tài khoản hiện tại
            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound("Không tìm thấy cửa hàng");

            // Lấy danh sách khuyến mãi của cửa hàng
            var danhSachKhuyenMai = await _context.KhuyenMai
                .Where(km => km.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View(danhSachKhuyenMai);
        }

        [HttpGet]
        [Route("Client/Shop/ThemMaKhuyenMai")]
        public IActionResult ThemMaKhuyenMai()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThemMaKhuyenMai")]
        public async Task<IActionResult> ThemMaKhuyenMai(KhuyenMaiModel_Client model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound("Không tìm thấy cửa hàng");

            var mucGiamGia = model.MucGiamGia / 100.0;

            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            INSERT INTO KhuyenMai
                (idCuaHang, maKhuyenMai, mucGiamGia, donToiThieu, soLuong, thoiGianBatDau, thoiGianKetThuc)
            VALUES
                (@idCuaHang, @maKhuyenMai, @mucGiamGia, @donToiThieu, @soLuong, @thoiGianBatDau, @thoiGianKetThuc)";
                cmd.Parameters.AddWithValue("@idCuaHang", cuaHang.IdCuaHang);
                cmd.Parameters.AddWithValue("@maKhuyenMai", model.MaKhuyenMai);
                cmd.Parameters.AddWithValue("@mucGiamGia", mucGiamGia);
                cmd.Parameters.AddWithValue("@donToiThieu", model.DonToiThieu);
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@thoiGianBatDau", model.ThoiGianBatDau);
                cmd.Parameters.AddWithValue("@thoiGianKetThuc", model.ThoiGianKetThuc);
                await cmd.ExecuteNonQueryAsync();
            }

            TempData["SuccessMessage"] = "Thêm mã khuyến mãi thành công!";
            return RedirectToAction("DanhSachKhuyenMai");
        }

        [HttpGet]
        [Route("Client/Shop/XoaKhuyenMai")]
        public async Task<IActionResult> XoaKhuyenMai(int id)
        {
            // Kiểm tra xem mã khuyến mãi có đang được sử dụng không
            bool isUsed = await _context.DonThue.AnyAsync(d => d.IdKhuyenMai == id);
            if (isUsed)
            {
                TempData["ErrorMessage"] = "Không thể xóa mã khuyến mãi này vì đang có đơn thuê sử dụng!";
                return RedirectToAction("DanhSachKhuyenMai");
            }

            var km = await _context.KhuyenMai.FirstOrDefaultAsync(x => x.IdKhuyenMai == id);
            if (km == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy mã khuyến mãi để xóa!";
                return RedirectToAction("DanhSachKhuyenMai");
            }

            _context.KhuyenMai.Remove(km);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa mã khuyến mãi thành công!";
            return RedirectToAction("DanhSachKhuyenMai");
        }

        [HttpGet]
        [Route("Client/Shop/SuaKhuyenMai")]
        public async Task<IActionResult> SuaKhuyenMai(int idKhuyenMai)
        {
            var km = await _context.KhuyenMai.FirstOrDefaultAsync(x => x.IdKhuyenMai == idKhuyenMai);
            if (km == null) return NotFound();
            return View(km);
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/SuaKhuyenMai")]
        public async Task<IActionResult> SuaKhuyenMai(KhuyenMaiModel_Client model)
        {
            var mucGiamGia = model.MucGiamGia / 100.0;
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
            UPDATE KhuyenMai SET
                maKhuyenMai = @maKhuyenMai,
                mucGiamGia = @mucGiamGia,
                donToiThieu = @donToiThieu,
                soLuong = @soLuong,
                thoiGianBatDau = @thoiGianBatDau,
                thoiGianKetThuc = @thoiGianKetThuc
            WHERE idKhuyenMai = @idKhuyenMai";
                cmd.Parameters.AddWithValue("@maKhuyenMai", model.MaKhuyenMai);
                cmd.Parameters.AddWithValue("@mucGiamGia", mucGiamGia);
                cmd.Parameters.AddWithValue("@donToiThieu", model.DonToiThieu);
                cmd.Parameters.AddWithValue("@soLuong", model.SoLuong);
                cmd.Parameters.AddWithValue("@thoiGianBatDau", model.ThoiGianBatDau);
                cmd.Parameters.AddWithValue("@thoiGianKetThuc", model.ThoiGianKetThuc);
                cmd.Parameters.AddWithValue("@idKhuyenMai", model.IdKhuyenMai);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật mã khuyến mãi thành công!";
            return RedirectToAction("DanhSachKhuyenMai");
        }

        [Route("Client")]
        [Route("Client/Shop/DanhSachLoaiXe")]
        public async Task<IActionResult> DanhSachLoaiXe()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var danhSachLoaiXe = await _context.LoaiXe
                .Where(lx => lx.IdCuaHang == cuaHang.IdCuaHang)
                .ToListAsync();

            return View(danhSachLoaiXe);
        }
        [HttpGet]
        [Route("Client/Shop/SuaLoaiXe")]
        public async Task<IActionResult> SuaLoaiXe(int id)
        {
            var loaiXe = await _context.LoaiXe.FirstOrDefaultAsync(lx => lx.IdLoaiXe == id);
            if (loaiXe == null) return NotFound();
            return View(loaiXe);
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/SuaLoaiXe")]
        public async Task<IActionResult> SuaLoaiXe(LoaiXeModel_Client model)
        {
            var loaiXe = await _context.LoaiXe.FirstOrDefaultAsync(lx => lx.IdLoaiXe == model.IdLoaiXe);
            if (loaiXe == null) return NotFound();

            loaiXe.TenLoaiXe = model.TenLoaiXe;
            _context.LoaiXe.Update(loaiXe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật loại xe thành công!";
            return RedirectToAction("DanhSachLoaiXe");
        }
        [HttpGet]
        [Route("Client/Shop/XoaLoaiXe")]
        public async Task<IActionResult> XoaLoaiXe(int id)
        {
            // Kiểm tra loại xe có đang được dùng cho xe nào không
            bool isUsed = await _context.Xe.AnyAsync(x => x.IdLoaiXe == id);
            if (isUsed)
            {
                TempData["ErrorMessage"] = "Không thể xóa loại xe này vì đang có xe sử dụng!";
                return RedirectToAction("DanhSachLoaiXe");
            }

            var loaiXe = await _context.LoaiXe.FirstOrDefaultAsync(lx => lx.IdLoaiXe == id);
            if (loaiXe == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy loại xe để xóa!";
                return RedirectToAction("DanhSachLoaiXe");
            }

            _context.LoaiXe.Remove(loaiXe);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa loại xe thành công!";
            return RedirectToAction("DanhSachLoaiXe");
        }
        [HttpGet]
        [Route("Client/Shop/ThemLoaiXe")]
        public ActionResult ThemLoaiXe()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThemLoaiXe")]
        public async Task<IActionResult> ThemLoaiXe(string tenLoaiXe)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound("Không tìm thấy cửa hàng");

            var loaiXe = new LoaiXeModel_Client
            {
                TenLoaiXe = tenLoaiXe,
                IdCuaHang = cuaHang.IdCuaHang
            };
            _context.LoaiXe.Add(loaiXe);
            await _context.SaveChangesAsync();

            return RedirectToAction("ThemXe");
        }

        // Hàm tính tổng doanh thu của 1 danh sách đơn thuê (đã có sẵn)
        private decimal TinhTongDoanhThuThang(List<DonThueModel_Client> danhSachDonThue)
        {
            decimal tongDoanhThu = 0;
            foreach (var don in danhSachDonThue)
            {
                decimal tongTien = 0;
                foreach (var ct in don.ChiTietDonThue)
                {
                    var thoiGianThue = (don.NgayTraXe - don.NgayNhanXe).TotalHours;
                    bool tinhTheoNgay = thoiGianThue > 23;
                    var soNgay = (int)Math.Ceiling((don.NgayTraXe - don.NgayNhanXe).TotalDays);
                    var soGio = (int)Math.Ceiling(thoiGianThue);

                    if (tinhTheoNgay)
                        tongTien += (ct.GiaThueTheoNgay ?? 0) * ct.SoLuong * soNgay;
                    else
                        tongTien += (ct.GiaThueTheoGio ?? 0) * ct.SoLuong * soGio;
                }
                if (don.KhuyenMai != null && don.KhuyenMai.MucGiamGia > 0)
                    tongTien = tongTien * (1 - Convert.ToDecimal(don.KhuyenMai.MucGiamGia));
                tongDoanhThu += tongTien;
            }
            return tongDoanhThu;
        }

        private List<ThongKeTheoNgay> TinhDoanhThuTheoNgay(List<DonThueModel_Client> danhSachDonThue, int month, int year)
        {
            // Tạo danh sách ngày trong tháng
            var daysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => new DateTime(year, month, day))
                .ToList();

            // Tính doanh thu từng ngày dựa trên ngày trả xe
            var doanhThuTheoNgay = danhSachDonThue
                .GroupBy(d => d.NgayTraXe.Date)
                .ToDictionary(g => g.Key, g => TinhTongDoanhThuThang(g.ToList()));

            // Ghép vào danh sách ngày, nếu không có thì doanh thu = 0
            var result = daysInMonth
                .Select(date => new ThongKeTheoNgay
                {
                    Ngay = date.ToString("yyyy-MM-dd"),
                    DoanhThu = doanhThuTheoNgay.ContainsKey(date) ? doanhThuTheoNgay[date] : 0
                })
                .ToList();

            return result;
        }

        [Route("Client/Shop/BaoCao")]
        public async Task<IActionResult> BaoCao(int? month, int? year)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
            if (cuaHang == null) return NotFound();
            int cuaHangId = cuaHang.IdCuaHang;

            int m = month ?? DateTime.Now.Month;
            int y = year ?? DateTime.Now.Year;

            // Lấy danh sách đơn hoàn thành trong tháng
            var donThueHoanThanh = await _context.DonThue
            .Include(d => d.ChiTietDonThue)
                .ThenInclude(ct => ct.Xe)
                    .ThenInclude(x => x.LoaiXe)
            .Include(d => d.KhuyenMai)
            .Include(d => d.User)
            .Where(d => d.TrangThaiDon == "Hoàn thành"
                && d.IdCuaHang == cuaHangId
                && d.NgayTraXe.Month == m
                && d.NgayTraXe.Year == y)
            .ToListAsync();

            // Tổng doanh thu tháng
            decimal tongDoanhThuThang = TinhTongDoanhThuThang(donThueHoanThanh);

            // Doanh thu theo ngày (đủ ngày trong tháng)
            var doanhThuTheoNgay = TinhDoanhThuTheoNgay(donThueHoanThanh, m, y);

            var chiTietDonThueList = _context.ChiTietDonThue
            .Include(ct => ct.DonThue)
                .ThenInclude(d => d.KhuyenMai)
            .Include(ct => ct.Xe)
                .ThenInclude(x => x.LoaiXe)
            .Where(ct => ct.DonThue.TrangThaiDon == "Hoàn thành"
                && ct.DonThue.IdCuaHang == cuaHangId
                && ct.DonThue.NgayTraXe.Month == m
                && ct.DonThue.NgayTraXe.Year == y)
            .AsEnumerable() // chuyển sang xử lý trên C#
            .ToList();



            var doanhThuTheoLoaiXe = chiTietDonThueList
    .GroupBy(ct => ct.Xe.LoaiXe.TenLoaiXe)
    .Select(g => new ThongKeLoaiXe
    {
        LoaiXe = g.Key,
        DoanhThu = g.Sum(ct =>
        {
            var thoiGianThue = (ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalHours;
            bool tinhTheoNgay = thoiGianThue > 23;
            var soNgay = (int)Math.Ceiling((ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalDays);
            var soGio = (int)Math.Ceiling(thoiGianThue);

            decimal tien = tinhTheoNgay
                ? (ct.GiaThueTheoNgay ?? 0m) * ct.SoLuong * soNgay
                : (ct.GiaThueTheoGio ?? 0m) * ct.SoLuong * soGio;

            decimal mucGiam = ct.DonThue.KhuyenMai != null ? (decimal)ct.DonThue.KhuyenMai.MucGiamGia : 0m;
            return tien * (1m - mucGiam);
        })
    })
    .ToList();

            var doanhThuCacXe = chiTietDonThueList
                .GroupBy(ct => ct.Xe.TenXe)
                .Select(g => new DoanhThuXe
                {
                    TenXe = g.Key,
                    DoanhThu = g.Sum(ct =>
                    {
                        var thoiGianThue = (ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalHours;
                        bool tinhTheoNgay = thoiGianThue > 23;
                        var soNgay = (int)Math.Ceiling((ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalDays);
                        var soGio = (int)Math.Ceiling(thoiGianThue);

                        decimal tien = tinhTheoNgay
                            ? (ct.GiaThueTheoNgay ?? 0m) * ct.SoLuong * soNgay
                            : (ct.GiaThueTheoGio ?? 0m) * ct.SoLuong * soGio;

                        decimal mucGiam = ct.DonThue.KhuyenMai != null ? (decimal)ct.DonThue.KhuyenMai.MucGiamGia : 0m;
                        return tien * (1m - mucGiam);
                    })
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            // Top 10 người dùng theo tổng tiền thuê
            var topNguoiDung = donThueHoanThanh
                .GroupBy(d => d.User.Ho + " " + d.User.Ten)
                .Select(g => new TopNguoiDung
                {
                    TenNguoiDung = g.Key,
                    TongTienThue = TinhTongDoanhThuThang(g.ToList())
                })
                .OrderByDescending(x => x.TongTienThue)
                .Take(10)
                .ToList();

            var model = new BaoCaoViewModel
            {
                SoXe = _dbHelper.LaySoXe(cuaHangId),
                SoXeDaThue = _dbHelper.LaySoXeDaThue(cuaHangId, m, y),
                SoDonDaThue = _dbHelper.LaySoDonDaThue(cuaHangId, m, y),
                DoanhThuThangNay = tongDoanhThuThang,
                DoanhThuTheoNgay = doanhThuTheoNgay,
                DoanhThuTheoLoaiXe = doanhThuTheoLoaiXe,
                DoanhThuCacXe = doanhThuCacXe,
                TopNguoiDung = topNguoiDung
            };
            var chiTietTheoLoaiXe = chiTietDonThueList
            .Select(ct => {
                var thoiGianThue = (ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalHours;
                bool tinhTheoNgay = thoiGianThue > 23;
                var soNgay = (int)Math.Ceiling((ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalDays);
                var soGio = (int)Math.Ceiling(thoiGianThue);
                decimal tien = tinhTheoNgay
                    ? (ct.GiaThueTheoNgay ?? 0m) * ct.SoLuong * soNgay
                    : (ct.GiaThueTheoGio ?? 0m) * ct.SoLuong * soGio;
                decimal mucGiam = ct.DonThue.KhuyenMai != null ? (decimal)ct.DonThue.KhuyenMai.MucGiamGia : 0m;
                decimal tongTien = tien * (1m - mucGiam);

                return new
                {
                    TenNguoiDat = ct.DonThue.User.Ho + " " + ct.DonThue.User.Ten,
                    TenXe = ct.Xe.TenXe, // Thêm dòng này
                    LoaiXe = ct.Xe.LoaiXe.TenLoaiXe,
                    TongTien = tongTien
                };
            })
            .ToList();

            ViewBag.ChiTietTheoLoaiXe = System.Text.Json.JsonSerializer.Serialize(chiTietTheoLoaiXe);

            var chiTietTheoNgay = chiTietDonThueList
            .Where(ct => ct.DonThue.TrangThaiDon == "Hoàn thành")
            .GroupBy(ct => ct.DonThue.NgayTraXe.Date)
            .ToDictionary(
                g => g.Key.ToString("yyyy-MM-dd"),
                g => g.Select(ct => {
                    var thoiGianThue = (ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalHours;
                    bool tinhTheoNgay = thoiGianThue > 23;
                    var soNgay = (int)Math.Ceiling((ct.DonThue.NgayTraXe - ct.DonThue.NgayNhanXe).TotalDays);
                    var soGio = (int)Math.Ceiling(thoiGianThue);
                    decimal tien = tinhTheoNgay
                        ? (ct.GiaThueTheoNgay ?? 0m) * ct.SoLuong * soNgay
                        : (ct.GiaThueTheoGio ?? 0m) * ct.SoLuong * soGio;
                    decimal mucGiam = ct.DonThue.KhuyenMai != null ? (decimal)ct.DonThue.KhuyenMai.MucGiamGia : 0m;
                    decimal tongTien = tien * (1m - mucGiam);

                    return new
                    {
                        TenNguoiDat = ct.DonThue.User.Ho + " " + ct.DonThue.User.Ten,
                        TenXe = ct.Xe.TenXe,
                        LoaiXe = ct.Xe.LoaiXe.TenLoaiXe,
                        SoLuong = ct.SoLuong,
                        TongTien = tongTien
                    };
                }).ToList()
            );
            ViewBag.ChiTietTheoNgay = System.Text.Json.JsonSerializer.Serialize(chiTietTheoNgay);

            return View(model);
        }

        [Route("Client/Shop/ChiTietDonThue")]
        public async Task<IActionResult> ChiTietDonThue(int id)
        {
            var don = await _context.DonThue
                .Include(d => d.User)
                .Include(d => d.KhuyenMai)
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .FirstOrDefaultAsync(d => d.IdDonThue == id);

            if (don == null) return NotFound();

            return View(don);
        }

        [Route("Client/Shop/NhanTinShop")]
        public async Task<IActionResult> NhanTinShop(int? id)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Tìm IdCuaHang theo IdTaiKhoan (userId)
            var cuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(ch => ch.IdTaiKhoan.ToString() == userId);

            if (cuaHang == null)
                return NotFound("Không tìm thấy cửa hàng liên quan đến tài khoản này");

            int IdCuaHang = cuaHang.IdCuaHang;

            // Lấy danh sách đoạn chat của shop
            var dsDoanChat = _context.DoanChat
            .Include(dc => dc.TaiKhoan)
            .Where(dc => dc.IdCuaHang == IdCuaHang)
            .Select(dc => new DoanChatViewModel
            {
                IdDoanChat = dc.IdDoanChat,
                TaiKhoan = new TaiKhoanModel_Client
                {
                    IdTaiKhoan = dc.TaiKhoan.Id,
                    Ho = dc.TaiKhoan.Ho,
                    Ten = dc.TaiKhoan.Ten,
                    SoDienThoai = dc.TaiKhoan.SoDienThoai,
                    HinhAnh = dc.TaiKhoan.HinhAnh,
                    VaiTro = dc.TaiKhoan.VaiTro,
                    TrangThaiTaiKhoan = dc.TaiKhoan.TrangThai
                },
                IdTaiKhoanCuaHang = dc.TaiKhoan.Id
            })
            .ToList();


            if (!dsDoanChat.Any())
            {
                // Nếu không có đoạn chat nào, trả về view với danh sách rỗng
                return View(new DanhSachVaChiTietChatViewModel
                {
                    DanhSachDoanChat = dsDoanChat,
                    ChiTietDoanChat = null
                });
            }

            // Nếu id không truyền hoặc id không thuộc dsDoanChat thì mặc định lấy đoạn chat đầu tiên
            int idDoanChat = id ?? dsDoanChat.First().IdDoanChat;
            if (!dsDoanChat.Any(dc => dc.IdDoanChat == idDoanChat))
                idDoanChat = dsDoanChat.First().IdDoanChat;

            // Lấy chi tiết đoạn chat đang active
            var doanChat = _context.DoanChat
                .Include(dc => dc.TinNhans)
                .Include(dc => dc.CuaHang)
                .Include(dc => dc.TaiKhoan)
                .FirstOrDefault(dc => dc.IdDoanChat == idDoanChat);

            var chiTietDoanChat = doanChat == null ? null : new DoanChatViewModel
            {
                IdDoanChat = doanChat.IdDoanChat,
                TaiKhoan = new TaiKhoanModel_Client
                {
                    IdTaiKhoan = doanChat.TaiKhoan.Id,
                    Ho = doanChat.TaiKhoan.Ho,
                    Ten = doanChat.TaiKhoan.Ten,
                    SoDienThoai = doanChat.TaiKhoan.SoDienThoai,
                    HinhAnh = doanChat.TaiKhoan.HinhAnh,
                    VaiTro = doanChat.TaiKhoan.VaiTro,
                    TrangThaiTaiKhoan = doanChat.TaiKhoan.TrangThai
                },
                TenNguoiDung = doanChat.CuaHang.TenCuaHang,
                TinNhans = doanChat.TinNhans.OrderBy(t => t.ThoiGianGui).ToList(),
                IdTaiKhoanCuaHang = doanChat.TaiKhoan.Id
            };


            // Trả về model tổng hợp chứa danh sách đoạn chat và chi tiết đoạn chat
            var vmTongHop = new DanhSachVaChiTietChatViewModel
            {
                DanhSachDoanChat = dsDoanChat,
                ChiTietDoanChat = chiTietDoanChat
            };

            return View(vmTongHop);
        }

        [Route("Client/Shop/GuiNhanTinShop")]
        [HttpPost]
        public IActionResult GuiTinNhan(int idDoanChat, string tinNhanMoi)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var tn = new TinNhanModel
            {
                IdDoanChat = idDoanChat,
                IdTaiKhoanGui = userId,
                NoiDung = tinNhanMoi,
                ThoiGianGui = DateTime.Now
            };

            _context.TinNhan.Add(tn);
            _context.SaveChanges();

            return Ok(new { Success = true });
        }

        [Route("Client/Shop/DanhSachDoanChat")]
        [HttpGet]
        public async Task<IActionResult> DanhSachDoanChat()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Tìm IdCuaHang theo IdTaiKhoan (userId)
            var cuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(ch => ch.IdTaiKhoan.ToString() == userId);

            if (cuaHang == null)
                return NotFound("Không tìm thấy cửa hàng liên quan đến tài khoản này");

            int IdCuaHang = cuaHang.IdCuaHang;

            // Lấy danh sách đoạn chat của user
            var dsDoanChat = _context.DoanChat
                .Include(dc => dc.TaiKhoan) // Already included
                .Where(dc => dc.IdCuaHang == IdCuaHang)
                .Select(dc => new DoanChatViewModel
                {
                    IdDoanChat = dc.IdDoanChat,
                    CuaHang = dc.CuaHang,
                })
                .ToList();


            return View(dsDoanChat);
        }
    }
}   