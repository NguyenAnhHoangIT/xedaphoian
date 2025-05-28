using ThueXeDapHoiAn.Areas.Admin.Models;

namespace ThueXeDapHoiAn.Areas.Admin.ViewModels
{
    public class ThongKeViewModel
    {
        public int SoLuongTaiKhoan { get; set; }
        public int SoLuongXe { get; set; }
        public int SoLuongCuaHang { get; set; }
        public decimal TongDoanhThuThangNay { get; set; }

        public List<UserTopModel> TopNguoiDungTheoTienThue { get; set; }
        public List<StoreRevenueModel> DoanhThuCuaHangTheoNgay { get; set; }
        public List<StoreTopModel> TopCuaHangTheoDoanhThu { get; set; }
        public List<VehicleRankingModel> XepHangXeTheoSoLuong { get; set; }
        public List<VehicleRevenueModel> DoanhThuTheoLoaiXe { get; set; }
    }
    public class UserTopModel
    {
        public string TenNguoiDung { get; set; }
        public decimal TongTienThue { get; set; }
    }
    public class StoreRevenueModel
    {
        public string Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public string TenCuaHang { get; set; }
    }
    public class StoreTopModel
    {
        public string TenCuaHang { get; set; }
        public decimal DoanhThu { get; set; }
        public int Hang { get; set; }
    }
    public class VehicleRankingModel
    {
        public string TenXe { get; set; }
        public int SoLuong { get; set; }
        public int Hang { get; set; }
    }
    public class VehicleRevenueModel
    {
        public string TenXe { get; set; }
        public decimal DoanhThu { get; set; }
        public string Color { get; set; }
    }
}
