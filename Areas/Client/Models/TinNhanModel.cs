using System.ComponentModel.DataAnnotations;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class TinNhanModel
    {
        [Key]
        public int IdTinNhan { get; set; }
        public int IdDoanChat { get; set; }
        public int IdTaiKhoanGui { get; set; }
        public string NoiDung { get; set; }
        public DateTime ThoiGianGui { get; set; }

        public DoanChatModel DoanChat { get; set; }
        public UserModel TaiKhoanGui { get; set; }
    }
}
