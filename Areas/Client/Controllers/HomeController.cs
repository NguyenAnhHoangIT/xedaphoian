using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class HomeController : Controller
    {
        [Route("Client")]
        [Route("Client/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ThongTinTaiKhoan")]
        public IActionResult ThongTinTaiKhoan()
        {
            return View();
        }


        [Route("Client")]
        [Route("Client/DangKyCuaHang")]
        public IActionResult DangKyCuaHang()
        {
            return View();
        }

        [Authorize]
        [Route("Client/CuaHangRedirect")]
        public IActionResult CuaHangRedirect()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Shop")
            {
                return RedirectToAction("ThongTinCuaHang", "CuaHang", new { area = "Client" });
            }
            else
            {
                return RedirectToAction("DangKyCuaHang", "Home", new { area = "Client" });
            }
        }

        [Route("Client/DangXuat")]
        [Route("dangxuat")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

    }
}
