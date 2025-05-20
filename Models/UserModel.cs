using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("TaiKhoan")]
    public class UserModel
    {
        [Key]
        [Column("idTaiKhoan")]
        public int UserId { get; set; }
        [Column("ho")]
        public string Ho { get; set; }
        [Column("ten")]
        public string Ten { get; set; }
        [Column("soDienThoai")]

        public string SoDienThoai { get; set; }
        [Column("matKhau")]

        public string MatKhau { get; set; }
        [Column("hinhAnh")]

        public string HinhAnh { get; set; }
        [Column("vaiTro")]

        public string VaiTro { get; set; }
        [Column("trangThaiTaiKhoan")]

        public bool TrangThai { get; set; }

    }
}