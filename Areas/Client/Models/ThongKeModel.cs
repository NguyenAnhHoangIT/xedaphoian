using System;
using System.Collections.Generic;

namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DoanhThuTheoNgay
    {
        public DateTime Ngay { get; set; }
        public string NgayISO { get; set; } // Thêm dòng này
        public decimal DoanhThu { get; set; }
        public int SoDon { get; set; }
    }
    public class ThongKeViewModel
    {
        public int TongDon { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonHuy { get; set; }
        public decimal DoanhThu { get; set; }
        public List<DoanhThuTheoNgay> DoanhThuTheoNgay { get; set; }
    }
}