namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class DonThueViewModel
    {
        public int IdDonThue { get; set; }
        public string TenCuaHang { get; set; }
        public string TrangThai { get; set; }

        public List<ChiTietDonThueViewModel> DanhSachXe { get; set; } = new List<ChiTietDonThueViewModel>();
    }
}
