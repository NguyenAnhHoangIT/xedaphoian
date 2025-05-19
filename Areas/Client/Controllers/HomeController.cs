using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;

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
            var dsXe = _context.Xe.ToList();

            var danhGiaJoin = (from dg in _context.DanhGia
                               join dt in _context.DonThue on dg.IdDonThue equals dt.IdDonThue
                               select new { dg.DiemDanhGia, dt.IdCuaHang }).ToList();

            var dsCuaHang = _context.CuaHang
                .ToList()
                .Select(ch => new CuaHang_Index_ViewModel_Client
                {
                    IdCuaHang = ch.IdCuaHang,
                    TenCuaHang = ch.TenCuaHang,
                    MoTa = ch.GioiThieu,
                    HinhAnh = ch.HinhAnh,
                    DiemTrungBinh = danhGiaJoin
                        .Where(x => x.IdCuaHang == ch.IdCuaHang)
                        .Select(x => x.DiemDanhGia)
                        .DefaultIfEmpty(0)
                        .Average()
                })
                .ToList();


            var viewModel = new CuaHang_Xe_ViewModel_Client
            {
                DanhSachXe = dsXe,
                DanhSachCuaHang = dsCuaHang
            };

            return View(viewModel);
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
