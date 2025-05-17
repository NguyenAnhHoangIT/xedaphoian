using Microsoft.AspNetCore.Mvc;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;
using ThueXeDapHoiAn.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ThueXeDapHoiAn.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _databaseHelper;

        public AccountController(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        [Route("/account/dangnhap")]
        public IActionResult DangNhap()
        {
            return View();
        }

        [Route("/account/dangky")]
        public IActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        [Route("/account/dangnhap")]
        public async Task<IActionResult> DangNhap(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _databaseHelper.GetUserByPhoneNumber(model.SoDienThoai);

                if (user != null)
                {
                    // Verify password manually
                    if (user.MatKhau == model.Password)
                    {
                        string role = user.VaiTro;

                        if (role == "Client")
                        {
                            var cuaHangStatus = _databaseHelper.GetTrangThaiCuaHang(int.Parse(user.Id));
                            if (cuaHangStatus == 1) // Chỉ khi cửa hàng đã được duyệt
                            {
                                role = "Shop";
                            }
                        }


                        // ✅ Set up claims
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.SoDienThoai),
                    new Claim(ClaimTypes.Role, role) // Đã cập nhật thành "Shop" nếu cần
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        // ✅ Redirect based on role
                        if (role == "Admin")
                        {
                            return RedirectToAction("TaiKhoan", "Home", new { area = "Admin" });
                        }
                        else if (role == "Shop")
                        {
                            // Nếu là Shop thì trực tiếp vào trang DanhSachXe
                            return RedirectToAction("DanhSachXe", "CuaHang", new { area = "Client" });
                        }
                        else // Nếu là Client mà chưa có cửa hàng thì vào trang chủ
                        {
                            return RedirectToAction("Index", "Home", new { area = "Client" });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Sai mật khẩu!");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại!");
                }
            }

            return View(model);
        }




    }
}
