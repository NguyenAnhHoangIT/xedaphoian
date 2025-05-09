using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string SoDienThoai { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }
    }
}
