using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Ho { get; set; }

        [Required]
        public string Ten { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}