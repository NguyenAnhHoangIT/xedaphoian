using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class TinhNangController : Controller
    {
        [Route("Client")]
        [Route("Client/NhanTinClient")]
        public IActionResult NhanTinClient()
        {
            return View();
        }
    }
}
