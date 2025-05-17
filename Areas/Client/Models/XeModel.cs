using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("Xe")]
    public class XeModel
    {
        // Khóa chính
        [Key]
        public int idXe { get; set; }

        // Liên kết với cửa hàng
        public int idCuaHang { get; set; }

        // Liên kết với loại xe
        public int idLoaiXe { get; set; }

        // Tên xe
        [Required]
        [MaxLength(100)]
        public string tenXe { get; set; }

        // Giá thuê theo giờ
        [Column(TypeName = "decimal(10, 2)")]
        public decimal giaThueTheoGio { get; set; }

        // Giá thuê theo ngày
        [Column(TypeName = "decimal(10, 2)")]
        public decimal giaThueTheoNgay { get; set; }

        // Số lượng
        public int soLuong { get; set; }

        // Giới thiệu về xe
        public string gioiThieu { get; set; }

        // Hình ảnh của xe
        public string hinhAnh { get; set; }

        // Trạng thái loại xe
        public bool trangThaiLoaiXe { get; set; }

        // Liên kết với bảng CuaHang
        [ForeignKey("idCuaHang")]
        public virtual CuaHangModel CuaHang { get; set; }

        // Liên kết với bảng LoaiXe
        [ForeignKey("idLoaiXe")]
        public virtual LoaiXeModel LoaiXe { get; set; }
    }
}
