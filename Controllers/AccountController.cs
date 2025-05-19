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
                    // Verify the password manually
                    if (user.MatKhau == model.Password)
                    {
                        // ✅ Set up claims
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.SoDienThoai),
                    new Claim("HoTen", user.Ho + " " + user.Ten),
                    new Claim("idTaiKhoan", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.VaiTro) // "Admin" or "Client"
                };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // ✅ Sign in using cookie authentication
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        // ✅ Redirect based on role
                        if (user.VaiTro == "Admin")
                        {
                            return RedirectToAction("TaiKhoan", "Home", new { area = "Admin" });
                        }
                        else if (user.VaiTro == "Client" || user.VaiTro == "Shop")
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
