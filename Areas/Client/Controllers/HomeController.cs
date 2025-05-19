using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class HomeController : Controller
    {
        private readonly AppDbContextClient _context;
        public HomeController(AppDbContextClient context)
        {
            _context = context;
        }
        [Route("Client")]
        [Route("Client/Index")]
        public IActionResult Index()
        {

            var xe = _context.Xe.ToList();
            return View(xe);
        }
        [Route("Client")]
        [Route("Client/Search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var xe = await _context.Xe.Where(p => p.TenXe.Contains(searchTerm) || p.GioiThieu.Contains(searchTerm))
                .ToListAsync();
            ViewBag.Keyword = searchTerm;
            return View(xe);

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
