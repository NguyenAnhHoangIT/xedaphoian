namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class DonThueViewModel_Client
    {
        public int IdDonThue { get; set; }
        public string TenCuaHang { get; set; }
        public string TrangThai { get; set; }

        public List<ChiTietDonThueViewModel_Client> DanhSachXe { get; set; } = new List<ChiTietDonThueViewModel_Client>();
    }
}
