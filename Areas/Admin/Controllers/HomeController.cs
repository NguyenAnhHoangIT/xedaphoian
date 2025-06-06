﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Areas.Admin.Models;
using ThueXeDapHoiAn.Areas.Admin.ViewModels;
using ThueXeDapHoiAn.Data;

namespace ThueXeDapHoiAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly DatabaseHelperAdmin _dbHelper;
        private readonly AppDbContextClient _context;

        public HomeController(DatabaseHelperAdmin dbHelper, AppDbContextClient context)
        {
            _dbHelper = dbHelper;
            _context = context;
        }

        [Route("Admin")]
        [Route("Admin/TaiKhoan")]
        public IActionResult TaiKhoan()
        {
            var users = _dbHelper.GetAllUsers();

            return View(users);
        }
        [HttpPost]
        [Route("Admin/CapNhatTaiKhoan")]
        public IActionResult CapNhatTaiKhoan(TaiKhoanModel model)
        {
            bool result = _dbHelper.UpdateUser(model);
            return Json(new { success = result });
        }

        [HttpPost]
        [Route("Admin/ThemTaiKhoan")]
        public JsonResult ThemTaiKhoan(TaiKhoanModel user)
        {
            var result = _dbHelper.InsertUser(user);
            return Json(new { success = result });
        }

        [Route("Admin/DanhGiaBiBaoCao")]
        public IActionResult DanhGiaBiBaoCao()
        {
            return View();
        }

        [Route("Admin/ThongTinTaiKhoan")]
        public IActionResult ThongTinTaiKhoan()
        {
            var userId = User.FindFirst("ID")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // ✅ Fetch user information from the database
                var user = _dbHelper.GetUserById(Convert.ToInt32(userId));

                if (user != null)
                {
                    return View(user); // Pass the user model to the view
                }
            }

            // 🔸 If the user is not found, redirect to login
            return RedirectToAction("DangNhap", "Account");
        }
        [HttpPost]
        [Route("Admin/CapNhatThongTinTaiKhoan")]
        public IActionResult CapNhatThongTinTaiKhoan([FromBody] TaiKhoanModel model)
        {
            bool result = _dbHelper.UpdateUser(model); // Reuse your UpdateUser method
            return Json(new { success = result });
        }


        [Route("Admin/DangXuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Route("Admin/DuyetDangKyTaoCuaHang")]
        public IActionResult DuyetDangKyTaoCuaHang()
        {
            var pendingStores = _dbHelper.GetPendingCuaHang();
            return View(pendingStores);
        }
        [HttpPost]
        [Route("Admin/Home/ChapNhanCuaHang")]
        public IActionResult ChapNhanCuaHang([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("idCuaHang", out var idCuaHangProperty))
                {
                    return Json(new { success = false, message = "Không tìm thấy idCuaHang" });
                }

                int idCuaHang = 0;
                if (idCuaHangProperty.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    idCuaHang = idCuaHangProperty.GetInt32();
                }
                else
                {
                    return Json(new { success = false, message = "idCuaHang không hợp lệ" });
                }

                bool result = _dbHelper.ChapNhanCuaHang(idCuaHang);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy cửa hàng hoặc cập nhật thất bại" });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Admin/Home/TuChoiCuaHang")]
        public IActionResult TuChoiCuaHang([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("idCuaHang", out var idCuaHangProperty))
                {
                    return Json(new { success = false, message = "Không tìm thấy idCuaHang" });
                }

                int idCuaHang = 0;
                if (idCuaHangProperty.ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    idCuaHang = idCuaHangProperty.GetInt32();
                }
                else
                {
                    return Json(new { success = false, message = "idCuaHang không hợp lệ" });
                }

                bool result = _dbHelper.TuChoiCuaHang(idCuaHang);
                if (!result)
                {
                    return Json(new { success = false, message = "Không tìm thấy cửa hàng hoặc xóa thất bại" });
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi server: {ex.Message}" });
            }
        }


        [Route("Admin/Xe_CuaHang")]
        public IActionResult Xe_CuaHang()
        {
            var cuaHangWithBikes = _dbHelper.GetStoresWithBikes();
            return View(cuaHangWithBikes);
        }
        [HttpPost]
        [Route("Admin/Home/ChangeTrangThaiXe")]
        public IActionResult ChangeTrangThaiXe([FromBody] System.Text.Json.JsonElement data)
        {
            if (!data.TryGetProperty("idXe", out var idXeProperty))
            {
                return Json(new { success = false, message = "Không tìm thấy idXe" });
            }

            int idXe = 0;
            if (idXeProperty.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                idXe = idXeProperty.GetInt32();
            }
            else
            {
                return Json(new { success = false, message = "idXe không hợp lệ" });
            }

            bool result = _dbHelper.ChangeTrangThaiXe(idXe);

            return Json(new { success = result });
        }

        [Route("Admin/ThongKe")]
        public IActionResult ThongKe()
        {
            var model = new ThongKeViewModel
            {

                SoLuongTaiKhoan = _dbHelper.GetUserCount(),
                SoLuongXe = _dbHelper.GetVehicleCount(),
                SoLuongCuaHang = _dbHelper.GetStoreCount(),
                TongDoanhThuThangNay = _dbHelper.GetMonthlyRevenue(),

                TopNguoiDungTheoTienThue = _dbHelper.GetTopUsersByRentalAmount(10),
                DoanhThuCuaHangTheoNgay = _dbHelper.GetStoreRevenueCurrentMonth(),
                TopCuaHangTheoDoanhThu = _dbHelper.GetTopStoresByRevenue(5),
                XepHangXeTheoSoLuong = _dbHelper.GetVehicleRanking(10),
                DoanhThuTheoLoaiXe = _dbHelper.GetVehicleRevenue()
            };

            return View(model);
        }


    }
}
