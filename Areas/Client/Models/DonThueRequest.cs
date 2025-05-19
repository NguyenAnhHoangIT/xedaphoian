namespace ThueXeDapHoiAn.Areas.Client.Models
{
    public class DonThueRequest
    {
        public int IdTaiKhoan { get; set; }
        public int IdCuaHang { get; set; }
        public DateTime NgayNhanXe { get; set; }
        public DateTime NgayTraXe { get; set; }
        public int? IdKhuyenMai { get; set; }
        public string? GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string TrangThaiDon { get; set; }
        public string Mode { get; set; }
        public List<ChiTietDonThueRequest> ChiTietDonThue { get; set; } // Đây là danh sách chi tiết
    }
}
