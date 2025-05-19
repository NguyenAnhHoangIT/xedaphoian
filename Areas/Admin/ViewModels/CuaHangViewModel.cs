namespace ThueXeDapHoiAn.Areas.Admin.ViewModels
{
    public class CuaHangViewModel
    {
        public int Id { get; set; }
        public string TenCuaHang { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public List<XeViewModel> XeList { get; set; }
    }
}
