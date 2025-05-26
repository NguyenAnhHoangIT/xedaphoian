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
                            var cuaHangStatus = _databaseHelper.GetTrangThaiCuaHang(int.Parse(user.UserId.ToString()));
                            if (cuaHangStatus == "True") // Chỉ khi cửa hàng đã được duyệt
                            {
                                role = "Shop";
                            }
                        }
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.SoDienThoai),
                    new Claim(ClaimTypes.Role, user.VaiTro), // "Admin" or "Client"
                    new Claim("ID", user.UserId.ToString()),
                    new Claim("Avatar", user.HinhAnh ?? "avatar-default.jpg"),
                    new Claim("FullName", $"{user.Ho} {user.Ten}"),
                    new Claim("HoTen", $"{user.Ho} {user.Ten}"),
                    new Claim("idTaiKhoan", user.UserId.ToString()),
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
