using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Areas.Repository;
using ThueXeDapHoiAn.Models;
using System.Security.Claims;
using ThueXeDapHoiAn.Data;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Data;
using System.Text.Json;
using System.Globalization;
namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class XemChiTietController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly AppDbContextClient _context;
        private readonly DatabaseHelper _databaseHelper;
        public XemChiTietController(AppDbContextClient context, DatabaseHelper databaseHelper,PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
            _context = context;
            _databaseHelper = databaseHelper;
        }

        [Route("Client")]
        [Route("Client/ChiTietCuaHang")]
        public async Task<IActionResult> ChiTietCuaHang(int id)
        {
            var cuaHang = await _context.CuaHang.FindAsync(id);
            if (cuaHang == null)
            {
                return NotFound();
            }

            var danhSachXe = await _context.Xe
                .Where(x => x.IdCuaHang == id)
                .ToListAsync();

            var diemTrungBinh = await (
                from dg in _context.DanhGia
                join dt in _context.DonThue on dg.IdDonThue equals dt.IdDonThue
                where dt.IdCuaHang == id
                select (double?)dg.DiemDanhGia
            ).AverageAsync();

            var viewModel = new ChiTietCuaHangViewModel_Client
            {
                CuaHang = cuaHang,
                DanhSachXe = danhSachXe,
                DiemTrungBinh = diemTrungBinh
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetXeTheoLoai(int idCuaHang, List<int> idLoaiXe)
        {
            var xeQuery = _context.Xe.AsQueryable();

            xeQuery = xeQuery.Where(x => x.IdCuaHang == idCuaHang);

            if (idLoaiXe != null && idLoaiXe.Any())
            {
                xeQuery = xeQuery.Where(x => idLoaiXe.Contains(x.IdLoaiXe));
            }

            var danhSachXe = await xeQuery.Select(x => new
            {
                x.IdXe,
                x.TenXe,
                x.GioiThieu,
                x.GiaThueTheoGio,
                x.HinhAnh
            }).ToListAsync();

            return Json(danhSachXe);
        }


        [Route("Client")]
        [Route("Client/ChiTietXe")]
        public async Task<IActionResult> ChiTietXe(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var xe = _context.Xe.FirstOrDefault(c => c.IdXe == id);
            if (xe == null)
            {
                return NotFound();
            }

            var xeCungCuaHang = _context.Xe
                .Where(x => x.IdCuaHang == xe.IdCuaHang && x.IdXe != id)
                .ToList();

            var loaiXeDict = _context.LoaiXe
                .ToDictionary(l => l.IdLoaiXe, l => l.TenLoaiXe);

            ViewBag.XeCungCuaHang = xeCungCuaHang;
            ViewBag.LoaiXeDict = loaiXeDict;

            // ✅ Lấy thông tin cửa hàng: Giới thiệu, Tên, Hình ảnh
            var cuaHangInfo = _context.CuaHang
                .Where(ch => ch.IdCuaHang == xe.IdCuaHang)
                .Select(ch => new
                {
                    ch.GioiThieu,
                    ch.TenCuaHang,
                    ch.HinhAnh
                })
                .FirstOrDefault();

            ViewBag.GioiThieuCuaHang = cuaHangInfo?.GioiThieu;
            ViewBag.TenCuaHang = cuaHangInfo?.TenCuaHang;
            ViewBag.HinhAnhCuaHang = cuaHangInfo?.HinhAnh;

            var danhGiaList = await _context.DanhGia
                .Include(dg => dg.DonThue).ThenInclude(dt => dt.CuaHang)
                .Include(dg => dg.DonThue).ThenInclude(dt => dt.User)
                .Where(dg => dg.DonThue.CuaHang.IdCuaHang == xe.IdCuaHang)
                .OrderByDescending(dg => dg.ThoiGianDanhGia)
                .ToListAsync();

            double diemTrungBinh = 0;
            if (danhGiaList.Any())
            {
                diemTrungBinh = Math.Round(danhGiaList.Average(dg => dg.DiemDanhGia), 1);
            }
            ViewBag.DiemTrungBinh = diemTrungBinh;


            ViewBag.DanhGiaList = danhGiaList;

            return View(xe);
        }



        [HttpPost]
        [Route("Client/ThemXe")]
        public async Task<IActionResult> Add(int id)
        {
            var xe = await _context.Xe.FindAsync(id);
            if (xe == null)
            {
                return Json(new { success = false, message = "Xe không tồn tại" });
            }

            var cart = HttpContext.Session.GetJson<List<ChiTietDonThueModel_Client>>("Cart") ?? new List<ChiTietDonThueModel_Client>();

            // Kiểm tra nếu giỏ hàng đã có ít nhất 1 xe
            if (cart.Any())
            {
                // Lấy id cửa hàng của xe đầu tiên trong giỏ hàng
                var idCuaHangFirstItem = cart.First().IdCuaHang;

                // Kiểm tra nếu xe mới có idCuaHang khác với idCuaHang của xe đầu tiên trong giỏ
                if (xe.IdCuaHang != idCuaHangFirstItem)
                {
                    return Json(new { success = false, message = "Bạn không được đặt các xe khác cửa hàng!" });
                }
            }

            // Tìm xem xe đã có trong giỏ chưa
            var cartItem = cart.FirstOrDefault(c => c.IdXe == id);
            if (cartItem == null)
            {
                cart.Add(new ChiTietDonThueModel_Client(xe)); // constructor đã gán IdCuaHang
            }
            else
            {
                cartItem.SoLuong += 1;
            }

            // Lưu lại giỏ hàng vào session
            HttpContext.Session.SetJson("Cart", cart);

            return Json(new { success = true, message = "Đã thêm xe vào giỏ" });
        }




        [HttpPost]
        [Route("Client/XoaXe")]
        public IActionResult Delete(int id)
        {
            var cart = HttpContext.Session.GetJson<List<ChiTietDonThueModel_Client>>("Cart") ?? new List<ChiTietDonThueModel_Client>();
            var cartItem = cart.FirstOrDefault(c => c.IdXe == id);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["error"] = "Xóa xe thành công";
            return RedirectToAction("DatXe");
        }



        [HttpGet]
        public JsonResult KiemTraTinhTrang(DateTime tu, DateTime den, int idXe)
        {
            // Tổng số lượng xe ban đầu
            int tongSoLuong = _context.Xe.FirstOrDefault(x => x.IdXe == idXe)?.SoLuong ?? 0;

            // Lọc các đơn thuê có thời gian giao nhau và chứa xe cần kiểm tra
            var soLuongDaThue = (from ct in _context.ChiTietDonThue
                                 join dt in _context.DonThue on ct.IdDonThue equals dt.IdDonThue
                                 where ct.IdXe == idXe &&
                                       !(dt.NgayTraXe <= tu || dt.NgayNhanXe >= den)
                                 select ct.SoLuong).Sum();

            int soLuongConLai = Math.Max(tongSoLuong - soLuongDaThue, 0);

            return Json(new { soLuongConLai });
        }


        [Route("Client")]
        [Route("Client/DatXe")]
        [HttpGet]
        public IActionResult DatXe()
        {
            List<ChiTietDonThueModel_Client> CTDT_Items = HttpContext.Session.GetJson<List<ChiTietDonThueModel_Client>>("Cart") ?? new List<ChiTietDonThueModel_Client>();

            string tenCuaHang = "";
            int? idCuaHang = null;

            if (CTDT_Items.Any())
            {
                // Lấy IdCuaHang từ phần tử đầu tiên trong giỏ hàng
                idCuaHang = CTDT_Items.First().IdCuaHang;

                // Kiểm tra tất cả các phần tử trong giỏ hàng có cùng IdCuaHang không
                if (CTDT_Items.All(item => item.IdCuaHang == idCuaHang))
                {
                    tenCuaHang = _context.CuaHang
                        .Where(ch => ch.IdCuaHang == idCuaHang)
                        .Select(ch => ch.TenCuaHang)
                        .FirstOrDefault() ?? "";
                }
                else
                {
                    // Trường hợp giỏ hàng có nhiều cửa hàng
                    tenCuaHang = "Nhiều cửa hàng"; // Hoặc có thể xử lý hiển thị danh sách các cửa hàng
                }
            }

            // Gán dữ liệu vào ViewModel và trả về View
            CTDonThueItemViewModel_Client xeVM = new()
            {
                CartItems = CTDT_Items,
                TenCuaHang = tenCuaHang,
                IdCuaHang = idCuaHang.HasValue ? idCuaHang.Value : 0
            };

            return View(xeVM);
        }


        [Route("Client")]
        [Route("Client/PhuongThucThanhToan")]
        public IActionResult PhuongThucThanhToan()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;  // Lấy ID người dùng
            var soDienThoai = User.FindFirst(ClaimTypes.Name)?.Value;  // Lấy số điện thoại từ claim
            var hoTen = User.FindFirst("HoTen")?.Value;  // Lấy tên người dùng từ claim (nếu đã thêm claim "HoTen")

            // Kiểm tra nếu không lấy được thông tin người dùng
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(hoTen))
            {
                return RedirectToAction("Login", "Account");
            }

            // Sử dụng số điện thoại để lấy thông tin người dùng
            var userModel = _databaseHelper.GetUserByPhoneNumber(soDienThoai);

            if (userModel == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Đưa thông tin vào ViewBag
            ViewBag.HoTen = hoTen;
            ViewBag.SDT = soDienThoai;
            ViewBag.UserModel = userModel;
            ViewBag.Id = userModel.Id;  // Đưa Id vào ViewBag
            ViewBag.PaypalClientId = _paypalClient.ClientId;

            return View();
        }




        [HttpGet]
        [Route("api/khuyenmai/check")]
        public IActionResult CheckPromoCode(string maKhuyenMai, int idCuaHang)
        {
            var km = _context.KhuyenMai
                .FirstOrDefault(k =>
                    k.MaKhuyenMai == maKhuyenMai &&
                    k.IdCuaHang == idCuaHang &&
                    k.ThoiGianBatDau <= DateTime.Now &&
                    k.ThoiGianKetThuc >= DateTime.Now &&
                    k.TrangThaiKhuyenMai == true &&
                    k.SoLuong > 0
                );

            if (km != null)
            {
                return Ok(new
                {
                    success = true,
                    idKhuyenMai = km.IdKhuyenMai, // THÊM DÒNG NÀY
                    maKhuyenMai = km.MaKhuyenMai,
                    mucGiamGia = km.MucGiamGia,
                    moTa = $"Giảm {km.MucGiamGia:#,##0}% cho đơn tối thiểu {km.DonToiThieu:#,##0}đ",
                });
            }

            return NotFound(new { success = false, message = "Mã không hợp lệ hoặc đã hết hạn." });
        }

        [HttpPost]
        [Route("thanhtoan/taicuahang")]
        public async Task<IActionResult> ThanhToan([FromBody] DonThueRequest_Client model)
        {
            if (model == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ! (Model is null)" });
            }

            try
            {
                // Kiểm tra các trường dữ liệu không được null
                if (model.IdTaiKhoan == null || model.IdCuaHang == null || model.NgayNhanXe == null || model.NgayTraXe == null)
                {
                    return Json(new { success = false, message = "Một số trường dữ liệu bị thiếu! (Missing fields)" });
                }

                // Kiểm tra tính hợp lệ của ngày nhận và trả xe
                DateTime ngayNhanXeParsed;
                if (!DateTime.TryParse(model.NgayNhanXe.ToString(), out ngayNhanXeParsed))
                {
                    return Json(new { success = false, message = "Ngày nhận xe không hợp lệ! (Invalid NgayNhanXe)" });
                }

                DateTime ngayTraXeParsed;
                if (!DateTime.TryParse(model.NgayTraXe.ToString(), out ngayTraXeParsed))
                {
                    return Json(new { success = false, message = "Ngày trả xe không hợp lệ! (Invalid NgayTraXe)" });
                }

                if (ngayNhanXeParsed.TimeOfDay == TimeSpan.Zero)
                {
                    ngayNhanXeParsed = ngayNhanXeParsed.Date.AddHours(7);
                }
                if (ngayTraXeParsed.TimeOfDay == TimeSpan.Zero)
                {
                    ngayTraXeParsed = ngayTraXeParsed.Date.AddHours(7);
                }

                // Kiểm tra giá trị của IdKhuyenMai nếu có
                if (model.IdKhuyenMai != null && model.IdKhuyenMai <= 0)
                {
                    return Json(new { success = false, message = "Mã khuyến mãi không hợp lệ! (Invalid IdKhuyenMai)" });
                }

                var donThue = new DonThueModel_Client
                {
                    UserId = model.IdTaiKhoan,
                    IdCuaHang = model.IdCuaHang,
                    NgayNhanXe = ngayNhanXeParsed,
                    NgayTraXe = ngayTraXeParsed,
                    IdKhuyenMai = model.IdKhuyenMai,
                    GhiChu = model.GhiChu,
                    PhuongThucThanhToan = model.PhuongThucThanhToan,
                    ThoiGianTao = DateTime.Now,
                    TrangThaiDon = model.TrangThaiDon
                };

                _context.DonThue.Add(donThue);
                await _context.SaveChangesAsync();
                int idDonThueMoi = donThue.IdDonThue;

                // Thêm chi tiết đơn thuê
                foreach (var chiTiet in model.ChiTietDonThue)
                {
                    var chiTietModel = new ChiTietDonThueModel_Client
                    {
                        IdDonThue = idDonThueMoi,
                        IdXe = chiTiet.IdXe,
                        SoLuong = chiTiet.SoLuong,
                        GiaThueTheoGio = model.Mode == "hourly" ? chiTiet.DonGia : null,
                        GiaThueTheoNgay = model.Mode == "daily" ? chiTiet.DonGia : null
                    };

                    _context.ChiTietDonThue.Add(chiTietModel);
                }

                await _context.SaveChangesAsync();

                // Xóa session Cart
                HttpContext.Session.Remove("Cart");

                return Json(new { success = true, message = "Đặt thuê thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                return Json(new { success = false, message = "Có lỗi khi lưu dữ liệu: " + dbEx.InnerException?.Message ?? dbEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/PhuongThucThanhToan/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder([FromBody] JsonElement data, CancellationToken cancellationToken)
        {
            try
            {
                if (!data.TryGetProperty("tongTien", out var tongTienProperty))
                {
                    return BadRequest(new { message = "Yêu cầu không chứa 'tongTien'." });
                }

                decimal tongTienVND = tongTienProperty.GetDecimal();
                if (tongTienVND <= 0)
                {
                    return BadRequest(new { message = "Tổng tiền phải lớn hơn 0." });
                }

                // Chuyển đổi VND sang USD
                decimal tiGiaUSD = 24000m; // Tỷ giá hiện tại
                decimal tongTienUSD = tongTienVND / tiGiaUSD;
                string tongTienStr = tongTienUSD.ToString("F2", CultureInfo.InvariantCulture); // Ví dụ: "6.25"

                string donViTienTe = "USD";
                string maDonHangThamChieu = "DH" + DateTime.Now.Ticks;

                var response = await _paypalClient.CreateOrder(tongTienStr, donViTienTe, maDonHangThamChieu);
                if (response == null || string.IsNullOrWhiteSpace(response.id))
                {
                    return BadRequest(new { message = "Không thể tạo đơn hàng với PayPal." });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Lỗi máy chủ khi tạo đơn hàng.",
                    detail = ex.GetBaseException().Message
                });
            }
        }



        [Authorize]
        [HttpPost("/PhuongThucThanhToan/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, string tempOrderID, [FromBody] DonThueRequest_Client model, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderID))
            {
                return BadRequest(new { message = "orderID không hợp lệ." });
            }

            if (string.IsNullOrWhiteSpace(tempOrderID))
            {
                return BadRequest(new { message = "Không tìm thấy mã đơn hàng tạm thời." });
            }

            try
            {
                // Gọi phương thức CaptureOrder và nhận về CaptureOrderResponse
                var response = await _paypalClient.CaptureOrder(orderID);

                // Kiểm tra nếu phản hồi không hợp lệ
                if (response == null || string.IsNullOrWhiteSpace(response.id))
                {
                    return BadRequest(new { message = "Lỗi khi xác nhận thanh toán với PayPal." });
                }

                // Kiểm tra trạng thái thanh toán từ phản hồi PayPal
                if (response.status != "COMPLETED") // Nếu bạn muốn kiểm tra trạng thái thanh toán là "COMPLETED"
                {
                    return BadRequest(new { message = "Thanh toán chưa hoàn tất." });
                }

                // Tiến hành lưu đơn hàng vào cơ sở dữ liệu
                var donThue = new DonThueModel_Client
                {
                    UserId = model.IdTaiKhoan,
                    IdCuaHang = model.IdCuaHang,
                    NgayNhanXe = model.NgayNhanXe,
                    NgayTraXe = model.NgayTraXe,
                    IdKhuyenMai = model.IdKhuyenMai,
                    GhiChu = model.GhiChu,
                    PhuongThucThanhToan = model.PhuongThucThanhToan,
                    ThoiGianTao = DateTime.Now,
                    TrangThaiDon = model.TrangThaiDon
                };

                _context.DonThue.Add(donThue);
                await _context.SaveChangesAsync();
                int idDonThueMoi = donThue.IdDonThue;

                // Lưu chi tiết đơn thuê
                foreach (var chiTiet in model.ChiTietDonThue)
                {
                    var chiTietModel = new ChiTietDonThueModel_Client
                    {
                        IdDonThue = idDonThueMoi,
                        IdXe = chiTiet.IdXe,
                        SoLuong = chiTiet.SoLuong,
                        GiaThueTheoGio = model.Mode == "hourly" ? chiTiet.DonGia : null,
                        GiaThueTheoNgay = model.Mode == "daily" ? chiTiet.DonGia : null
                    };

                    _context.ChiTietDonThue.Add(chiTietModel);
                }

                await _context.SaveChangesAsync();

                // Xóa session Cart
                HttpContext.Session.Remove("Cart");

                return Ok(new { success = true, message = "Thanh toán thành công và đơn hàng đã được lưu!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Lỗi khi xác nhận đơn hàng.",
                    detail = ex.GetBaseException().Message
                });
            }
        }

        #endregion






        [Route("Client")]
        [Route("Client/ThanhToan")]
        public IActionResult ThanhToan()
        {
            return View();
        }
    }
}
