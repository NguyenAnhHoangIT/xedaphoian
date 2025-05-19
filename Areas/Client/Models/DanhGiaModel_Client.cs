using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DanhGiaModel
    {
        [Key]
        public int IdDanhGia { get; set; }
        public int IdDonThue{ get; set; }
        public double DiemDanhGia { get; set; }
        public string NoiDungDanhGia{ get; set; }
        public int IdHinhAnh{ get; set; }
        public DateTime ThoiGiaDanhGia{ get; set; }
        public bool TrangThaiDanhGia{ get; set; }
    }
}
