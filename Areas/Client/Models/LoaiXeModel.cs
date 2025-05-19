using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Models;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class LoaiXeModel
    {
        [Key]
        public int IdLoaiXe { get; set; }
        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }
        public string TenLoaiXe { get; set; }

        public CuaHangModel CuaHang { get; set; }
    }
}
