using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class XemChiTietController : Controller
    {
        [Route("Client")]
        [Route("Client/ChiTietCuaHang")]
        public IActionResult ChiTietCuaHang()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ChiTietXe")]
        public IActionResult ChiTietXe()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/DatXe")]
        public IActionResult DatXe()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/PhuongThucThanhToan")]
        public IActionResult PhuongThucThanhToan()
        {
            return View();
        }

        [Route("Client")]
        [Route("Client/ThanhToan")]
        public IActionResult ThanhToan()
        {
            return View();
        }
    }
}
