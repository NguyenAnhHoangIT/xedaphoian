namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class CuaHangViewModel
    {
        public int IdCuaHang { get; set; }
        public int IdTaiKhoan { get; set; }   // string để view hiển thị
        public string TenCuaHang { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Gmail { get; set; }
        public string GioiThieu { get; set; }
        public string HinhAnh { get; set; }
        public bool TrangThaiCuaHang { get; set; }
    }

}
