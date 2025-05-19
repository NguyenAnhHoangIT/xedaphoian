using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("LoaiXe")]
    public class LoaiXeModel_cuaHang
    {
        // Khóa chính
        [Key]
        public int idLoaiXe { get; set; }

        // Liên kết với cửa hàng
        public int idCuaHang { get; set; }

        // Tên loại xe
        [Required]
        [MaxLength(100)]
        public string tenLoaiXe { get; set; }

        // Liên kết với bảng CuaHang
        [ForeignKey("idCuaHang")]
        public virtual CuaHangModel_cuaHang CuaHang { get; set; }
    }
}
