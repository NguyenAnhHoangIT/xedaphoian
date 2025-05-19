using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Models;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class LoaiXeModel_Client
    {
        [Key]
        public int IdLoaiXe { get; set; }
        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }
        public string TenLoaiXe { get; set; }

        public CuaHangModel_Client CuaHang { get; set; }
    }
}
