using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Data;

namespace ThueXeDapHoiAn.Areas.Client.ViewComponents
{
    public class GiaXeViewComponent : ViewComponent
    {
        private readonly AppDbContextClient _context;
        public GiaXeViewComponent(AppDbContextClient context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Xe.AsQueryable();

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.GiaThueTheoGio >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.GiaThueTheoGio <= maxPrice.Value);
            }

            var result = await query.ToListAsync();

            return View(result);
        }

    }
}
