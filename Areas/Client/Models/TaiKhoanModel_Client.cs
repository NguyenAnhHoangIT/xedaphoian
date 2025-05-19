using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class TaiKhoanModel_Client
    {
        [Key]
        public int IdTaiKhoan { get; set; }

        public string Ho { get; set; }
        public string Ten { get; set; }
        public string SoDienThoai { get; set; }
        public string MatKhau { get; set; }
        public string HinhAnh { get; set; }
        public string VaiTro { get; set; }
        public bool TrangThaiTaiKhoan { get; set; }

        // Nếu cần có quan hệ với DonThueModel
        public ICollection<DonThueModel_Client> DonThue { get; set; }
    }
}
