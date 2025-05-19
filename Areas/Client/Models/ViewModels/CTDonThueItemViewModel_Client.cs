namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class CTDonThueItemViewModel_Client
    {
        public List<ChiTietDonThueModel_Client> CartItems { get; set; }
        public decimal TongTien { get; set; }
        public int IdCuaHang { get; set; }
        public string TenCuaHang { get; set; } = string.Empty;
    }
}
