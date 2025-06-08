using Microsoft.AspNetCore.Mvc;

namespace ThueXeDapHoiAn.Areas.Client.ViewComponents
{
    public class GiaXe2ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string selectedRange = "", string sortOrder = "")
        {
            var priceRanges = new List<(string Label, string Value)>
            {
                ("0 - 50.000đ", "0-50000"),
                ("51.000đ - 100.000đ", "51000-100000"),
                ("101.000đ - 200.000đ", "101000-200000"),
                ("Trên 200.000đ", "200000+"),
                ("Tuỳ chọn", "custom")
            };

            ViewBag.SelectedRange = selectedRange;
            ViewBag.SortOrder = sortOrder;

            return View(priceRanges);
        }
    }
}
