using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        [Route("Admin")]
        [Route("Admin/TaiKhoan")]
        public IActionResult TaiKhoan()
        {
            return View();
        }

        [Route("Admin")]
        [Route("Admin/DanhGiaBiBaoCao")]
        public IActionResult DanhGiaBiBaoCao()
        {
            return View();
        }

        [Route("Admin")]
        [Route("Admin/ThongTinTaiKhoan")]
        public IActionResult ThongTinTaiKhoan()
        {
            return View();
        }

        [Route("Admin/DangXuat")]
        [Route("dangxuatAdmin")]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Route("Admin")]
        [Route("Admin/DuyetDangKyTaoCuaHang")]
        public IActionResult DuyetDangKyTaoCuaHang()
        {
            return View();
        }
    }
}
