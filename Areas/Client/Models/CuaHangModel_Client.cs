using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Areas.Repository.Validation;
using ThueXeDapHoiAn.Models;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class CuaHangModel_Client
    {
        [Key]
        public int IdCuaHang { get; set; }
        [ForeignKey("User")]
        public int IdTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên cửa hàng")]
        [MinLength(4, ErrorMessage = "Tên cửa hàng phải có ít nhất 4 ký tự")]
        public string TenCuaHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [MinLength(4, ErrorMessage = "Địa chỉ phải có ít nhất 4 ký tự")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải gồm 10 chữ số")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [MinLength(4, ErrorMessage = "Email phải có ít nhất 4 ký tự")]
        public string Gmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giới thiệu")]
        [MinLength(4, ErrorMessage = "Giới thiệu phải có ít nhất 4 ký tự")]
        public string GioiThieu { get; set; }

        public string HinhAnh { get; set; }
        public string TrangThaiCuaHang { get; set; }

        public UserModel User { get; set; }
        [NotMapped]
        [FileExtension]
        [Required(ErrorMessage = "Vui lòng gửi hình ảnh")]
        public IFormFile IMageUpload { get; set; }
    }
}
