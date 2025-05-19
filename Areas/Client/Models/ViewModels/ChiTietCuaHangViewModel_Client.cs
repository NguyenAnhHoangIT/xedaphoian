namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class ChiTietCuaHangViewModel_Client
    {
        public CuaHangModel_Client CuaHang { get; set; }
        public List<XeModel_Client> DanhSachXe { get; set; }
        public double? DiemTrungBinh { get; set; }
    }
}
