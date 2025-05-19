namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class ChiTietDonThueViewModel_Client
    {
        public int IdDonThue { get; set; }
        public string TenCuaHang { get; set; }
        public string DiaChiCuaHang { get; set; }
        public string SdtCuaHang { get; set; }
        public List<ChiTietXe> DanhSachXe { get; set; }
        public DateTime NgayNhanXe { get; set; }
        public DateTime NgayTraXe { get; set; }
        public double SoGioThue { get; set; }
        public string MaKhuyenMai { get; set; }
        public string GhiChu { get; set; }
        public double MucGiamGia { get; set; }
        public decimal DonToiThieu { get; set; }

        public class ChiTietXe
        {
            public int IdXe { get; set; }
            public string TenXe { get; set; }
            public int SoLuong { get; set; }
            public decimal DonGia { get; set; }
        }
    }
}
