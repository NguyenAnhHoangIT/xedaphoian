using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class AppDbContextClient : DbContext
    {
        public AppDbContextClient(DbContextOptions<AppDbContextClient> options) : base(options) { }

        public DbSet<CuaHangModel> CuaHang { get; set; }
        public DbSet<XeModel> Xe { get; set; }
        public DbSet<LoaiXeModel> LoaiXe { get; set; }
        public DbSet<TaiKhoanModel> TaiKhoan { get; set; }
        public DbSet<KhuyenMaiModel> KhuyenMai { get; set; }
        public DbSet<DonThueModel> DonThue { get; set; }
        public DbSet<ChiTietDonThueModel> ChiTietDonThue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình composite primary key cho ChiTietDonThue
            modelBuilder.Entity<ChiTietDonThueModel>()
                .HasKey(ct => new { ct.idDonThue, ct.idXe });

            base.OnModelCreating(modelBuilder);
        }
    }
}
