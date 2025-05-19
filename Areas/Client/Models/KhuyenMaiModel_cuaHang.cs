using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("KhuyenMai")]
    public class KhuyenMaiModel_cuaHang
    {
        [Key]
        public int idKhuyenMai { get; set; }

        [ForeignKey("CuaHang")]
        public int idCuaHang { get; set; }

        [Required, MaxLength(50)]
        public string maKhuyenMai { get; set; }

        public double mucGiamGia { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal donToiThieu { get; set; }

        public int soLuong { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime thoiGianBatDau { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime thoiGianKetThuc { get; set; }

        public bool trangThaiKhuyenMai { get; set; }

    }
}
