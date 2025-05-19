using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class XeModel
    {
        [Key]
        public int IdXe { get; set; }
        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }
        [ForeignKey("LoaiXe")]
        public int IdLoaiXe { get; set; }

        public string TenXe { get; set; }

        public decimal GiaThueTheoGio { get; set; }

        public decimal GiaThueTheoNgay { get; set; }

        public int  SoLuong { get; set; }

        public string GioiThieu { get; set; }
        public string HinhAnh { get; set; }
        public bool TrangThaiLoaiXe { get; set; }

        public CuaHangModel CuaHang { get; set; }
        public LoaiXeModel LoaiXe { get; set; }

        public List<ChiTietDonThueModel> ChiTietDonThue { get; set; }
    }
}
