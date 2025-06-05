using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThueXeDapHoiAn.Areas.Client.Data;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Data;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class DonThueController : Controller
    {
        private readonly AppDbContextClient _context;
        private readonly ILogger<DonThueController> _logger;

        public DonThueController(AppDbContextClient context, ILogger<DonThueController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Route("Client")]
        [Route("Client/DonThue")]
        public IActionResult DonThue()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var listDonThue = _context.DonThue
                .Include(d => d.CuaHang)
                .Include(d => d.KhuyenMai)
                .Include(d => d.ChiTietDonThue)
                    .ThenInclude(ct => ct.Xe)
                .Where(d => d.UserId == userId)
                .Select(d => new DonThueViewModel_Client
                {
                    IdDonThue = d.IdDonThue,
                    TenCuaHang = d.CuaHang != null ? d.CuaHang.TenCuaHang : "Không xác định",
                    TrangThai = d.TrangThaiDon,
                    DanhSachXe = d.ChiTietDonThue != null
                        ? d.ChiTietDonThue.Select(ct => new ChiTietDonThueViewModel_Client
                        {
                            IdDonThue = d.IdDonThue,
                            TenCuaHang = d.CuaHang.TenCuaHang,
                            DiaChiCuaHang = d.CuaHang.DiaChi,
                            SdtCuaHang = d.CuaHang.SoDienThoai,
                            NgayNhanXe = d.NgayNhanXe,
                            NgayTraXe = d.NgayTraXe,
                            SoGioThue = (d.NgayTraXe - d.NgayNhanXe).TotalHours,
                            MaKhuyenMai = d.KhuyenMai != null ? d.KhuyenMai.MaKhuyenMai : null,
                            GhiChu = d.GhiChu,
                            MucGiamGia = d.KhuyenMai != null ? (double)d.KhuyenMai.MucGiamGia : 0.0,
                            DonToiThieu = d.KhuyenMai != null ? d.KhuyenMai.DonToiThieu : 0m,
                            DanhSachXe = new List<ChiTietDonThueViewModel_Client.ChiTietXe>
                            {
                        new ChiTietDonThueViewModel_Client.ChiTietXe
                        {
                            IdXe = ct.IdXe,
                            TenXe = ct.Xe != null ? ct.Xe.TenXe : "Không xác định",
                            SoLuong = ct.SoLuong,
                            DonGia = ct.GiaThueTheoNgay ?? 0
                        }
                            }
                        }).ToList()
                        : new List<ChiTietDonThueViewModel_Client>()
                })
                .OrderByDescending(d => d.IdDonThue) 
                .ToList();

            return View(listDonThue);
        }



        [Route("Client/DonThue/ChiTietDonThueChuaDuyet/{id}")]
        public async Task<IActionResult> ChiTietDonThueChuaDuyet(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var donThueEntity = await _context.DonThue
                .Include(o => o.CuaHang)
                .Include(o => o.ChiTietDonThue).ThenInclude(ct => ct.Xe)
                .Include(o => o.KhuyenMai)
                .FirstOrDefaultAsync(o => o.IdDonThue == id && o.UserId == userId);

            if (donThueEntity == null)
            {
                return NotFound();
            }

            var ngayNhan = donThueEntity.NgayNhanXe.Date;
            var ngayTra = donThueEntity.NgayTraXe.Date;
            var soGioThue = (donThueEntity.NgayTraXe - donThueEntity.NgayNhanXe).TotalHours;
            var soNgayThue = (ngayTra - ngayNhan).Days + 1;
            if (soNgayThue < 1) soNgayThue = 1;

            decimal tamTinh = 0;

            var chiTietDonThueList = new List<ChiTietDonThueModel_Client>();

            foreach (var ct in donThueEntity.ChiTietDonThue)
            {
                decimal giaThue = 0;

                if (soGioThue <= 23) // Thuê theo giờ
                {
                    giaThue = (ct.GiaThueTheoGio ?? 0) * (decimal)soGioThue;
                }
                else // Thuê theo ngày
                {
                    giaThue = (ct.GiaThueTheoNgay ?? 0) * soNgayThue;
                }

                tamTinh += giaThue * ct.SoLuong;

                chiTietDonThueList.Add(new ChiTietDonThueModel_Client
                {
                    IdDonThue = ct.IdDonThue,
                    IdXe = ct.IdXe,
                    SoLuong = ct.SoLuong,
                    GiaThueTheoGio = ct.GiaThueTheoGio,
                    GiaThueTheoNgay = ct.GiaThueTheoNgay,
                    Xe = ct.Xe
                });
            }

            // Tính khuyến mãi nếu có
            decimal mucGiamGia = (decimal)(donThueEntity.KhuyenMai?.MucGiamGia ?? 0);
            decimal giamGia = tamTinh * (mucGiamGia / 100);
            decimal tongCong = tamTinh - giamGia;

            var donThueViewModel = new DonThueViewModel2_Client
            {
                IdDonThue = donThueEntity.IdDonThue,
                IdTaiKhoan = userId,
                IdCuaHang = donThueEntity.IdCuaHang,
                NgayNhanXe = donThueEntity.NgayNhanXe,
                NgayTraXe = donThueEntity.NgayTraXe,
                MaKhuyenMai = donThueEntity.KhuyenMai?.MaKhuyenMai,
                GhiChu = donThueEntity.GhiChu,
                PhuongThucThanhToan = donThueEntity.PhuongThucThanhToan,
                ThoiGianTao = donThueEntity.ThoiGianTao,
                TrangThaiDon = donThueEntity.TrangThaiDon,
                DiaChiCuaHang = donThueEntity.CuaHang?.DiaChi ?? "Không có địa chỉ",
                SDTCuaHang = donThueEntity.CuaHang?.SoDienThoai ?? "Không có số điện thoại",
                ChiTietDonThue = chiTietDonThueList,
                TamTinh = tamTinh,
                MucGiamGia = mucGiamGia,
                GiamGia = giamGia,
                TongCong = tongCong
            };

            return View(donThueViewModel);
        }

        [Route("Client/DonThue/ChiTietDonThueChuaDuyet/HuyDon/{id}")]
        [HttpPost]
        public async Task<IActionResult> HuyDon(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Json(new { success = false, message = "Bạn chưa đăng nhập." });

            var donThue = await _context.DonThue
                .FirstOrDefaultAsync(o => o.IdDonThue == id && o.UserId == userId);

            if (donThue == null)
                return Json(new { success = false, message = "Không tìm thấy đơn." });

            if (donThue.TrangThaiDon != "Đang chờ duyệt")
                return Json(new { success = false, message = "Chỉ có thể hủy đơn thuê chưa duyệt." });

            // Cập nhật trạng thái đơn
            donThue.TrangThaiDon = "Đã hủy";

            _context.Update(donThue);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đơn đã được hủy thành công!" });
        }






        [Route("Client")]
        [Route("Client/DanhGia")]
        public IActionResult DanhGia()
        {
            return View();
        }
    }
}
