using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Models;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class KhuyenMaiModel
    {
        [Key]
        public int IdKhuyenMai { get; set; }

        [ForeignKey("CuaHang")]
        public int IdCuaHang { get; set; }  // Chỉ định rõ khóa ngoại

        public string MaKhuyenMai { get; set; }
        public double MucGiamGia { get; set; }
        public decimal DonToiThieu { get; set; }
        public int SoLuong { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public bool TrangThaiKhuyenMai { get; set; }

        public CuaHangModel CuaHang { get; set; }
    }

}
