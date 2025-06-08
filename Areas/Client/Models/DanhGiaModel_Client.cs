using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DanhGiaModel_Client
    {
        [Key]
        public int IdDanhGia { get; set; }

        [Required(ErrorMessage = "Id đơn thuê là bắt buộc")]
        public int IdDonThue { get; set; }

        [Required(ErrorMessage = "Bạn chưa chọn điểm đánh giá")]
        [Range(0.5, 5.0, ErrorMessage = "Điểm phải từ 0.5 đến 5 sao")]
        public double DiemDanhGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        public string NoiDungDanhGia { get; set; }

        public int? IdHinhAnh { get; set; }

        public DateTime ThoiGianDanhGia { get; set; }

        public bool TrangThaiDanhGia { get; set; }

        [ForeignKey("IdDonThue")]
        public virtual DonThueModel_Client DonThue { get; set; }

        [NotMapped]
        public virtual CuaHangModel_Client CuaHang => DonThue?.CuaHang;
    }

}
