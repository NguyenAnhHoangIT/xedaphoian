using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThueXeDapHoiAn.Data;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.ViewComponents
{
    public class UserProfileViewComponent : ViewComponent
    {
        private readonly AppDbContextClient _context;

        public UserProfileViewComponent(AppDbContextClient context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userIdStr = (User as ClaimsPrincipal)?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return View<UserModel>("Default", null); // ← sửa chỗ này
            }

            int userId = int.Parse(userIdStr);
            var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Id == userId);

            return View<UserModel>("Default", user); // ← sửa chỗ này
        }
    }
}
