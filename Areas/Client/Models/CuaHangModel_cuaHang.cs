using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThueXeDapHoiAn.Models
{
    [Table("CuaHang")]
    public class CuaHangModel_cuaHang
    {
        [Key]
        public int idCuaHang { get; set; }

        public int idTaiKhoan { get; set; }

        public string tenCuaHang { get; set; }

        public string diaChi { get; set; }

        public string soDienThoai { get; set; }

        public string gmail { get; set; }

        public string gioiThieu { get; set; }

        public string hinhAnh { get; set; }

        public string trangThaiCuaHang { get; set; }
    }
}
