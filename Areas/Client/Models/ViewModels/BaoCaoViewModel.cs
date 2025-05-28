namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class BaoCaoViewModel
    {
        public int SoXe { get; set; }
        public int SoXeDaThue { get; set; }
        public int SoDonDaThue { get; set; }
        public decimal DoanhThuThangNay { get; set; }

        public List<ThongKeTheoNgay> DoanhThuTheoNgay { get; set; }
        public List<ThongKeLoaiXe> DoanhThuTheoLoaiXe { get; set; }
        public List<DoanhThuXe> DoanhThuCacXe { get; set; }
        public List<TopNguoiDung> TopNguoiDung { get; set; }
    }

    public class ThongKeTheoNgay
    {
        public string Ngay { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class ThongKeLoaiXe
    {
        public string LoaiXe { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class DoanhThuXe
    {
        public string TenXe { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class TopNguoiDung
    {
        public string TenNguoiDung { get; set; }
        public decimal TongTienThue { get; set; }
    }


}
