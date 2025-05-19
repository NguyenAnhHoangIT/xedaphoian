namespace ThueXeDapHoiAn.Areas.Admin.ViewModels
{
    public class XeViewModel
    {
        public int Id { get; set; }
        public string TenXe { get; set; }
        public decimal GiaThueTheoGio { get; set; }
        public decimal GiaThueTheoNgay { get; set; }
        public int SoLuong { get; set; }
        public string GioiThieu { get; set; }
        public string HinhAnh { get; set; }
        public bool TrangThai { get; set; }
    }
}
