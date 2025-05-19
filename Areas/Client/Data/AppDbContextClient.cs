using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class AppDbContextClient : DbContext
    {
        public AppDbContextClient(DbContextOptions<AppDbContextClient> options) : base(options) { }

        public DbSet<CuaHangModel_cuaHang> CuaHang { get; set; }
        public DbSet<XeModel_cuaHang> Xe { get; set; }
        public DbSet<LoaiXeModel_cuaHang> LoaiXe { get; set; }
        public DbSet<TaiKhoanModel_cuaHang> TaiKhoan { get; set; }
        public DbSet<KhuyenMaiModel_cuaHang> KhuyenMai { get; set; }
        public DbSet<DonThueModel_cuaHang> DonThue { get; set; }
        public DbSet<ChiTietDonThueModel_cuaHang> ChiTietDonThue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình composite primary key cho ChiTietDonThue
            modelBuilder.Entity<ChiTietDonThueModel_cuaHang>()
                .HasKey(ct => new { ct.idDonThue, ct.idXe });

            base.OnModelCreating(modelBuilder);
        }
    }
}
