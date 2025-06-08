using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Data;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("api/chat")]
    [Authorize(Roles = "Client,Shop")]
    public class ChatApiController : Controller
    {
        private readonly AppDbContextClient _context;

        public ChatApiController(AppDbContextClient context)
        {
            _context = context;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveMessage([FromBody] TinNhanModel message)
        {
            message.ThoiGianGui = DateTime.Now;
            _context.TinNhan.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
