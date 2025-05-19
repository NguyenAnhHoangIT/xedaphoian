using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Models;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class CuaHangModel_Client
    {
        [Key]
        public int IdCuaHang { get; set; }
        [ForeignKey("User")]
        public int IdTaiKhoan { get; set; }
        public string TenCuaHang { get; set; }

        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Gmail { get; set; }
        public string GioiThieu { get; set; }
        public string HinhAnh { get; set; }
        public string TrangThaiCuaHang { get; set; }

        public UserModel User { get; set; }
    }
}
