using System.ComponentModel.DataAnnotations;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DoanChatModel
    {
        [Key]
        public int IdDoanChat { get; set; }
        public int IdTaiKhoan { get; set; }
        public int IdCuaHang { get; set; }
        public DateTime ThoiGianTao { get; set; }

        public UserModel TaiKhoan { get; set; }
        public CuaHangModel_Client CuaHang { get; set; }
        public virtual ICollection<TinNhanModel> TinNhans { get; set; }

    }
}
