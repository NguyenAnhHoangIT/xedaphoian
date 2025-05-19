using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DonThueEntity_Client
    {
        [Key]
        public int IdDonThue { get; set; }

        [ForeignKey("TaiKhoan")]  // Khóa ngoại tới TaiKhoanModel
        public int IdTaiKhoan { get; set; }  // Thay UserId bằng IdTaiKhoan

        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }

        public DateTime NgayNhanXe { get; set; }
        public DateTime NgayTraXe { get; set; }
        public int? IdKhuyenMai { get; set; }
        public string GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TrangThaiDon { get; set; }

        // Navigation properties
        public TaiKhoanModel_Client User { get; set; }
        public CuaHangModel_Client CuaHang { get; set; }
        public KhuyenMaiModel_Client KhuyenMai { get; set; }
        public ICollection<ChiTietDonThueModel_Client> ChiTietDonThue { get; set; }
    }
}
