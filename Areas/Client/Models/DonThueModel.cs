using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DonThueModel
    {
        [Key]
        public int IdDonThue { get; set; }

        [Column("idTaiKhoan")]
        public int UserId { get; set; }

        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }

        public DateTime NgayNhanXe { get; set; }

        public DateTime NgayTraXe { get; set; }

        [ForeignKey("KhuyenMai")]
        public int? IdKhuyenMai { get; set; }

        public string? GhiChu { get; set; }

        public string PhuongThucThanhToan { get; set; } = string.Empty;

        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        public string TrangThaiDon { get; set; } = "Đang chờ duyệt";

        public UserModel User { get; set; }

        public CuaHangModel CuaHang { get; set; }

        public KhuyenMaiModel KhuyenMai { get; set; }

        // Thêm thuộc tính ChiTietDonThue để thiết lập quan hệ 1-N với ChiTietDonThueModel
        public ICollection<ChiTietDonThueModel> ChiTietDonThue { get; set; }  // Đây là quan hệ một-nhiều
    }


}
