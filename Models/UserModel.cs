using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Ho { get; set; }
        public string Ten { get; set; }

        public string SoDienThoai { get; set; }

        public string MatKhau { get; set; }

        public string HinhAnh { get; set; }

        public string VaiTro { get; set; }

        public string TrangThai { get; set; }

    }
}
