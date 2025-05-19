using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("TaiKhoan")]
    public class TaiKhoanModel_cuaHang
    {
        [Key]
        public int idTaiKhoan { get; set; }

        [Required, MaxLength(50)]
        public string ho { get; set; }

        [Required, MaxLength(50)]
        public string ten { get; set; }

        [Required, MaxLength(10)]
        public string soDienThoai { get; set; }

        [Required, MaxLength(255)]
        public string matKhau { get; set; }

        [MaxLength(255)]
        public string? hinhAnh { get; set; }

        [Required, MaxLength(50)]
        public string vaiTro { get; set; }

        public bool trangThaiTaiKhoan { get; set; }
    }
}
