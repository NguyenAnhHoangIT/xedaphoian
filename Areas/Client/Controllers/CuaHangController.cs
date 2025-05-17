using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;
using Microsoft.Data.SqlClient;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Shop")]
    public class CuaHangController : Controller
    {
        private readonly AppDbContextClient _context;

        public CuaHangController(AppDbContextClient context)
        {
            _context = context;
        }
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThongTinCuaHang")]
        public async Task<IActionResult> ThongTinCuaHang()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang
                .FirstOrDefaultAsync(c => c.idTaiKhoan == userId);

            if (cuaHang == null)
            {
                return RedirectToAction("DangKyCuaHang", "Home", new { area = "Client" });
            }

            return View(cuaHang); // Truyền model sang view
        }
        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThongTinCuaHang")]
        public async Task<IActionResult> CapNhatThongTin(CuaHangModel model, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Xử lý ảnh đại diện
            string fileName = cuaHang.hinhAnh;
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
            cuaHang.tenCuaHang = model.tenCuaHang;
            cuaHang.diaChi = model.diaChi;
            cuaHang.soDienThoai = model.soDienThoai;
            cuaHang.gmail = model.gmail;
            cuaHang.gioiThieu = model.gioiThieu;
            cuaHang.hinhAnh = fileName;

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
            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Lấy danh sách xe của cửa hàng này
            var danhSachXe = await _context.Xe
                .Include(x => x.LoaiXe) // Bao gồm thông tin loại xe (LoạiXe)
                .Where(x => x.idCuaHang == cuaHang.idCuaHang) // Chỉ lấy xe của cửa hàng hiện tại
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

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Lọc loại xe theo idCuaHang của shop hiện tại
            ViewBag.LoaiXeList = await _context.LoaiXe
                .Where(lx => lx.idCuaHang == cuaHang.idCuaHang)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/ThemXe")]
        public async Task<IActionResult> ThemXe(XeModel model, IFormFile hinhAnh)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
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

            var xe = new XeModel
            {
                tenXe = model.tenXe,
                idLoaiXe = model.idLoaiXe,
                soLuong = model.soLuong,
                giaThueTheoGio = model.giaThueTheoGio,
                giaThueTheoNgay = model.giaThueTheoNgay,
                gioiThieu = model.gioiThieu,
                hinhAnh = fileName,
                idCuaHang = cuaHang.idCuaHang,
                trangThaiLoaiXe = true
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
            var exists = await _context.ChiTietDonThue.AnyAsync(ct => ct.idXe == id);
            if (exists)
            {
                TempData["ErrorMessage"] = "Không thể xóa xe này vì đã hoặc đang có đơn thuê liên quan!";
                return RedirectToAction("DanhSachXe");
            }

            var xe = await _context.Xe.FirstOrDefaultAsync(x => x.idXe == id);
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
            var xe = await _context.Xe.Include(x => x.LoaiXe).FirstOrDefaultAsync(x => x.idXe == id);
            if (xe == null) return NotFound();

            // Lấy idCuaHang từ xe hoặc từ user đăng nhập
            int idCuaHang = xe.idCuaHang;

            // Lọc loại xe theo idCuaHang của shop hiện tại
            ViewBag.LoaiXeList = await _context.LoaiXe
                .Where(lx => lx.idCuaHang == idCuaHang)
                .ToListAsync();

            return View(xe);
        }
        // ...existing code...

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/SuaXe")]
        public async Task<IActionResult> SuaXe(XeModel model, IFormFile hinhAnh)
        {
            // Lấy tên ảnh cũ từ database
            string fileName = null;
            var connectionString = _context.Database.GetConnectionString();
            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var getCmd = conn.CreateCommand();
                getCmd.CommandText = "SELECT hinhAnh FROM Xe WHERE idXe = @idXe";
                getCmd.Parameters.AddWithValue("@idXe", model.idXe);
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
                cmd.Parameters.AddWithValue("@tenXe", model.tenXe);
                cmd.Parameters.AddWithValue("@idLoaiXe", model.idLoaiXe);
                cmd.Parameters.AddWithValue("@soLuong", model.soLuong);
                cmd.Parameters.AddWithValue("@giaThueTheoGio", model.giaThueTheoGio);
                cmd.Parameters.AddWithValue("@giaThueTheoNgay", model.giaThueTheoNgay);
                cmd.Parameters.AddWithValue("@gioiThieu", (object?)model.gioiThieu ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hinhAnh", (object?)fileName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idXe", model.idXe);
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

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            // Truy vấn để lấy đơn thuê và chi tiết đơn thuê bao gồm thông tin xe
            var donThueChoDuyet = await _context.DonThue
                .Include(d => d.ChiTietDonThue)  // Bao gồm ChiTietDonThue
                    .ThenInclude(ct => ct.Xe)   // Bao gồm Xe
                    .ThenInclude(x => x.LoaiXe) // Bao gồm LoaiXe
                .Include(d => d.TaiKhoan)   // Bao gồm KhachHang
                .Include(d => d.KhuyenMai)
                .Where(d => d.trangThaiDon == "Chờ duyệt" && d.idCuaHang == cuaHang.idCuaHang)
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
                cmd.CommandText = @"UPDATE DonThue SET trangThaiDon = N'Đã duyệt' WHERE idDonThue = @id";
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

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var donThueDaDuyet = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.TaiKhoan)
                .Where(d => d.trangThaiDon == "Đã duyệt" && d.idCuaHang == cuaHang.idCuaHang)
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

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var donThueDaHuy = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.TaiKhoan)
                .Where(d => d.trangThaiDon == "Đã huỷ" && d.idCuaHang == cuaHang.idCuaHang)
                .ToListAsync();

            return View(donThueDaHuy);
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueHoanThanh")]
        public async Task<IActionResult> DonThueHoanThanh()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound();

            var donThueHoanThanh = await _context.DonThue
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                        .ThenInclude(x => x.LoaiXe)
                .Include(d => d.TaiKhoan)
                .Where(d => d.trangThaiDon == "Hoàn thành" && d.idCuaHang == cuaHang.idCuaHang)
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
            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound("Không tìm thấy cửa hàng");

            // Lấy danh sách khuyến mãi của cửa hàng
            var danhSachKhuyenMai = await _context.KhuyenMai
                .Where(km => km.idCuaHang == cuaHang.idCuaHang)
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
        public async Task<IActionResult> ThemMaKhuyenMai(KhuyenMaiModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var cuaHang = await _context.CuaHang.FirstOrDefaultAsync(c => c.idTaiKhoan == userId);
            if (cuaHang == null) return NotFound("Không tìm thấy cửa hàng");

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
                cmd.Parameters.AddWithValue("@idCuaHang", cuaHang.idCuaHang);
                cmd.Parameters.AddWithValue("@maKhuyenMai", model.maKhuyenMai);
                cmd.Parameters.AddWithValue("@mucGiamGia", model.mucGiamGia);
                cmd.Parameters.AddWithValue("@donToiThieu", model.donToiThieu);
                cmd.Parameters.AddWithValue("@soLuong", model.soLuong);
                cmd.Parameters.AddWithValue("@thoiGianBatDau", model.thoiGianBatDau);
                cmd.Parameters.AddWithValue("@thoiGianKetThuc", model.thoiGianKetThuc);
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
            bool isUsed = await _context.DonThue.AnyAsync(d => d.idKhuyenMai == id);
            if (isUsed)
            {
                TempData["ErrorMessage"] = "Không thể xóa mã khuyến mãi này vì đang có đơn thuê sử dụng!";
                return RedirectToAction("DanhSachKhuyenMai");
            }

            var km = await _context.KhuyenMai.FirstOrDefaultAsync(x => x.idKhuyenMai == id);
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
            var km = await _context.KhuyenMai.FirstOrDefaultAsync(x => x.idKhuyenMai == idKhuyenMai);
            if (km == null) return NotFound();
            return View(km);
        }

        [HttpPost]
        [Authorize(Roles = "Shop")]
        [Route("Client/Shop/SuaKhuyenMai")]
        public async Task<IActionResult> SuaKhuyenMai(KhuyenMaiModel model)
        {
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
                cmd.Parameters.AddWithValue("@maKhuyenMai", model.maKhuyenMai);
                cmd.Parameters.AddWithValue("@mucGiamGia", model.mucGiamGia);
                cmd.Parameters.AddWithValue("@donToiThieu", model.donToiThieu);
                cmd.Parameters.AddWithValue("@soLuong", model.soLuong);
                cmd.Parameters.AddWithValue("@thoiGianBatDau", model.thoiGianBatDau);
                cmd.Parameters.AddWithValue("@thoiGianKetThuc", model.thoiGianKetThuc);
                cmd.Parameters.AddWithValue("@idKhuyenMai", model.idKhuyenMai);
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật mã khuyến mãi thành công!";
            return RedirectToAction("DanhSachKhuyenMai");
        }

    }
}
