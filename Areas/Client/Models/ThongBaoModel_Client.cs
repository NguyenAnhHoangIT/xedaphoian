using System.ComponentModel.DataAnnotations;
using System;
namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class ThongBaoModel_Client
    {
        [Key]
        public int IdThongBao { get; set; }
        public int IdTaiKhoanNhan { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public bool LienKet { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public DateTime? ThoiGiamXem { get; set; }
    }
}
