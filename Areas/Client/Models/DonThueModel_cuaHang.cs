using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("DonThue")]
    public class DonThueModel_cuaHang
    {
        [Key]
        public int idDonThue { get; set; }

        public int idTaiKhoan { get; set; }
        public int idCuaHang { get; set; }
        public DateTime ngayNhanXe { get; set; }
        public DateTime ngayTraXe { get; set; }
        public int? idKhuyenMai { get; set; }
        public string ghiChu { get; set; }
        public string phuongThucThanhToan { get; set; }
        public DateTime thoiGianTao { get; set; }
        public string trangThaiDon { get; set; }

        // Navigation
        [ForeignKey("idTaiKhoan")]
        public virtual TaiKhoanModel_cuaHang TaiKhoan { get; set; }

        // Không cần Xe ở đây (DonThue không có idXe)
        // public virtual XeModel Xe { get; set; }

        public virtual ICollection<ChiTietDonThueModel_cuaHang> ChiTietDonThue { get; set; }
        [ForeignKey("idKhuyenMai")]
        public virtual KhuyenMaiModel_cuaHang KhuyenMai { get; set; }
    }
}
