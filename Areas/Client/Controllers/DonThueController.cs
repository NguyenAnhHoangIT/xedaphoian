using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class DonThueController : Controller
    {
        [Route("Client")]
        [Route("Client/DonThue")]
        public IActionResult DonThue()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ChiTietDonThue")]
        public IActionResult ChiTietDonThue()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ChiTietDonThueChuaDuyet")]
        public IActionResult ChiTietDonThueChuaDuyet()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/DanhGia")]
        public IActionResult DanhGia()
        {
            return View();
        }
    }
}
