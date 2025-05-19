namespace ThueXeDapHoiAn.Areas.Admin.Models
{
    public class XeModel
    {
        public int idXe { get; set; }
        public int idCuaHang { get; set; }
        public int idLoaiXe { get; set; }
        public string tenXe { get; set; }
        public decimal giaThueTheoGio { get; set; }
        public decimal giaThueTheoNgay { get; set; }
        public int soLuong { get; set; }
        public string gioiThieu { get; set; }
        public string hinhAnh { get; set; }
        public bool trangThaiLoaiXe { get; set; }
    }
}
