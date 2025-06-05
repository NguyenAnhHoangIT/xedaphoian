using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperClient
    {
        private readonly AppDbContextClient _context;
        private readonly string _connectionString;

        public DatabaseHelperClient(AppDbContextClient context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("Default");
        }

        public async Task<CuaHangModel_Client> GetCuaHangByUserId(int userId)
        {
            return await _context.CuaHang.FirstOrDefaultAsync(c => c.IdTaiKhoan == userId);
        }
        // Số xe của cửa hàng
        public int LaySoXe(int cuaHangId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM Xe WHERE IdCuaHang = @IdCuaHang";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public int LaySoXeDaThue(int cuaHangId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT SUM(ctdt.SoLuong)
            FROM ChiTietDonThue ctdt
            JOIN Xe xe ON ctdt.IdXe = xe.IdXe
            WHERE xe.IdCuaHang = @IdCuaHang";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public int LaySoDonDaThue(int cuaHangId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT COUNT(*)
            FROM DonThue
            WHERE IdCuaHang = @IdCuaHang AND (TrangThaiDon = 'Đã duyệt' OR TrangThaiDon = 'Hoàn thành')";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public decimal LayDoanhThuThangNay(int cuaHangId)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT SUM(
                CASE 
                    WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                    THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                    ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                END
            )
            FROM DonThue d
            JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
            JOIN Xe xe ON ctdt.IdXe = xe.IdXe
            WHERE d.IdCuaHang = @IdCuaHang
              AND MONTH(d.ThoiGianTao) = MONTH(GETDATE())
              AND YEAR(d.ThoiGianTao) = YEAR(GETDATE())
              AND (d.TrangThaiDon = 'Đã duyệt' OR d.TrangThaiDon = 'Hoàn thành')
            ";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // Doanh thu theo ngày trong tháng này
        public List<ThongKeTheoNgay> LayDoanhThuTheoNgay(int cuaHangId)
        {
            var result = new List<ThongKeTheoNgay>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            -- Lấy ngày đầu tháng hiện tại
            DECLARE @StartDate DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
            -- Lấy ngày cuối tháng hiện tại
            DECLARE @EndDate DATE = EOMONTH(GETDATE());

            WITH NgayTrongKhoang AS (
                SELECT @StartDate AS Ngay
                UNION ALL
                SELECT DATEADD(day, 1, Ngay)
                FROM NgayTrongKhoang
                WHERE Ngay < @EndDate
            ),
            DoanhThuTheoNgay AS (
                SELECT 
                    CAST(d.ThoiGianTao AS date) AS Ngay,
                    ISNULL(SUM(
                        CASE 
                            WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                            THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                            ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                        END
                    ), 0) AS DoanhThu
                FROM DonThue d
                JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
                JOIN Xe xe ON ctdt.IdXe = xe.IdXe
                WHERE d.IdCuaHang = @IdCuaHang
                  AND d.ThoiGianTao >= @StartDate
                  AND d.ThoiGianTao <= @EndDate
                  AND (d.TrangThaiDon = 'Đã duyệt' OR d.TrangThaiDon = 'Hoàn thành')
                GROUP BY CAST(d.ThoiGianTao AS date)
            )
            SELECT 
                n.Ngay,
                ISNULL(dt.DoanhThu, 0) AS DoanhThu
            FROM NgayTrongKhoang n
            LEFT JOIN DoanhThuTheoNgay dt ON n.Ngay = dt.Ngay
            ORDER BY n.Ngay
            OPTION (MAXRECURSION 0);
            ";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ThongKeTheoNgay
                {
                    Ngay = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                    DoanhThu = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                });
            }

            return result;
        }



        // Doanh thu theo loại xe
        public List<ThongKeLoaiXe> LayDoanhThuTheoLoaiXe(int cuaHangId)
        {
            var result = new List<ThongKeLoaiXe>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT lx.tenLoaiXe, 
                SUM(
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) AS DoanhThu
            FROM DonThue d
            JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
            JOIN Xe xe ON ctdt.IdXe = xe.IdXe
            JOIN LoaiXe lx ON xe.idLoaiXe = lx.idLoaiXe
            WHERE d.IdCuaHang = @IdCuaHang
              AND (d.TrangThaiDon = 'Đã duyệt' OR d.TrangThaiDon = 'Hoàn thành')
            GROUP BY lx.tenLoaiXe";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ThongKeLoaiXe
                {
                    LoaiXe = reader.GetString(0),
                    DoanhThu = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                });
            }

            return result;
        }

        // Doanh thu của từng xe
        public List<DoanhThuXe> LayDoanhThuCacXe(int cuaHangId)
        {
            var result = new List<DoanhThuXe>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT xe.TenXe,
                SUM(
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) AS DoanhThu
            FROM DonThue d
            JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
            JOIN Xe xe ON ctdt.IdXe = xe.IdXe
            WHERE d.IdCuaHang = @IdCuaHang
              AND (d.TrangThaiDon = 'Đã duyệt' OR d.TrangThaiDon = 'Hoàn thành')
            GROUP BY xe.TenXe
            ORDER BY DoanhThu DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DoanhThuXe
                {
                    TenXe = reader.GetString(0),
                    DoanhThu = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                });
            }

            return result;
        }

        // Top người dùng theo tổng tiền thuê
        public List<TopNguoiDung> LayTopNguoiDung(int cuaHangId)
        {
            var result = new List<TopNguoiDung>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
                    SELECT u.ho + ' ' + u.ten AS HoTen,
                SUM(
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) AS TongTien
            FROM DonThue d
            JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
            JOIN TaiKhoan u ON d.idTaiKhoan = u.idTaiKhoan
            WHERE d.IdCuaHang = @IdCuaHang
              AND (d.TrangThaiDon = N'Đã duyệt' OR d.TrangThaiDon = N'Hoàn thành')
            GROUP BY u.ho, u.ten
            ORDER BY TongTien DESC
            OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY
            ";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new TopNguoiDung
                {
                    TenNguoiDung = reader.GetString(0),
                    TongTienThue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                });
            }

            return result;
        }

    }

}
