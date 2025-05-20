using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class AppDbContextClient : DbContext
    {
        public AppDbContextClient(DbContextOptions<AppDbContextClient> options) : base(options) { }

        public DbSet<UserModel> TaiKhoan { get; set; }  // Your custom user table
        public DbSet<CuaHangModel_Client> CuaHang { get; set; }
        public DbSet<LoaiXeModel_Client> LoaiXe { get; set; }
        public DbSet<XeModel_Client> Xe { get; set; }
        public DbSet<ChiTietDonThueModel_Client> ChiTietDonThue { get; set; }
        public DbSet<DonThueModel_Client> DonThue { get; set; }
        public DbSet<KhuyenMaiModel_Client> KhuyenMai { get; set; }
        public DbSet<DanhGiaModel_Client> DanhGia { get; set; }
        public DbSet<ThongBaoModel_Client> ThongBao { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for ChiTietDonThueModel_Client
            modelBuilder.Entity<ChiTietDonThueModel_Client>()
                .HasKey(ct => new { ct.IdDonThue, ct.IdXe });

            // Relationships
            modelBuilder.Entity<ChiTietDonThueModel_Client>()
                .HasOne(ct => ct.DonThue)
                .WithMany(dt => dt.ChiTietDonThue)
                .HasForeignKey(ct => ct.IdDonThue);

            modelBuilder.Entity<ChiTietDonThueModel_Client>()
                .HasOne(ct => ct.Xe)
                .WithMany(x => x.ChiTietDonThue)
                .HasForeignKey(ct => ct.IdXe);

            modelBuilder.Entity<DonThueModel_Client>()
                .Property(d => d.UserId)
                .HasColumnName("idTaiKhoan");
            // Cấu hình composite primary key cho ChiTietDonThue
            modelBuilder.Entity<ChiTietDonThueModel_Client>()
                .HasKey(ct => new { ct.IdDonThue, ct.IdXe });

        }



    }

}
