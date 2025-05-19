namespace ThueXeDapHoiAn.Areas.Client.Models.ViewModels
{
    public class DonThueViewModel2
    {
        public int IdDonThue { get; set; }

        public int IdTaiKhoan { get; set; }  // Thay UserId bằng IdTaiKhoan

        public int IdCuaHang { get; set; }
        public DateTime NgayNhanXe { get; set; }
        public DateTime NgayTraXe { get; set; }
        public string SDTCuaHang { get; set; }
        public string? MaKhuyenMai { get; set; }
        public string GhiChu { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public DateTime ThoiGianTao { get; set; }
        public string TrangThaiDon { get; set; }
        public string DiaChiCuaHang { get; set; }
        public decimal TamTinh { get; set; }

        public decimal MucGiamGia { get; set; }

        public decimal GiamGia { get; set; }

        public decimal TongCong { get; set; }

        public ICollection<ChiTietDonThueModel> ChiTietDonThue { get; set; }
    }
}
