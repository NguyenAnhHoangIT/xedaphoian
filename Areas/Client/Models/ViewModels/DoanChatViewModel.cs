namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class DoanChatViewModel
    {
        public int IdDoanChat { get; set; }
        public CuaHangModel_Client CuaHang { get; set; }
        public string TenNguoiDung { get; set; }
        public List<TinNhanModel> TinNhans { get; set; }
        public string TinNhanMoi { get; set; }
        public int IdTaiKhoanCuaHang { get; set; }
        public TaiKhoanModel_Client TaiKhoan { get; set; }
    }
}
