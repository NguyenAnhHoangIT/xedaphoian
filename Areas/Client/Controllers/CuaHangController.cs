using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Shop")]
    public class CuaHangController : Controller
    {
        [Route("Client")]
        [Route("Client/Shop/ThongTinCuaHang")]
        public IActionResult ThongTinCuaHang()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DanhSachXe")]
        public IActionResult DanhSachXe()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/ThemXe")]
        public IActionResult ThemXe()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueChoDuyet")]
        public IActionResult DonThueChoDuyet()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueDaDuyet")]
        public IActionResult DonThueDaDuyet()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueDaHuy")]
        public IActionResult DonThueDaHuy()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DonThueHoanThanh")]
        public IActionResult DonThueHoanThanh()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/DanhSachKhuyenMai")]
        public IActionResult DanhSachKhuyenMai()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/Shop/ThemMaKhuyenMai")]
        public IActionResult ThemMaKhuyenMai()
        {
            return View();
        }

    }
}
