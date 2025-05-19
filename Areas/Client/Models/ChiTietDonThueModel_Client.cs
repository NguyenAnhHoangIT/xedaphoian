using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class ChiTietDonThueModel_Client
    {
        public int IdDonThue { get; set; }
        public int IdXe { get; set; }

        public int SoLuong { get; set; }

        public decimal? GiaThueTheoGio { get; set; }

        public decimal? GiaThueTheoNgay { get; set; }

        // Các thuộc tính sau đây không có trong CSDL → dùng [NotMapped] hoặc xóa nếu không cần

        [NotMapped]
        public string HinhAnh { get; set; }

        [NotMapped]
        public string TenXe { get; set; }

        [NotMapped]
        public int IdCuaHang { get; set; }

        [NotMapped]
        public DonThueModel_Client DonThue { get; set; }

        [NotMapped]
        public XeModel_Client Xe { get; set; }

        public ChiTietDonThueModel_Client() { }

        public ChiTietDonThueModel_Client(XeModel_Client xe)
        {
            IdXe = xe.IdXe;
            TenXe = xe.TenXe;
            HinhAnh = xe.HinhAnh;
            SoLuong = 1;
            GiaThueTheoGio = xe.GiaThueTheoGio;
            GiaThueTheoNgay = xe.GiaThueTheoNgay;
            IdCuaHang = xe.IdCuaHang;
        }
    }
}
