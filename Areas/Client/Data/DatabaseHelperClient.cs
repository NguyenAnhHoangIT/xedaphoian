using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public int LaySoXeDaThue(int cuaHangId, int month, int year)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT ISNULL(SUM(ctdt.SoLuong), 0)
        FROM ChiTietDonThue ctdt
        JOIN DonThue d ON ctdt.IdDonThue = d.IdDonThue
        JOIN Xe xe ON ctdt.IdXe = xe.IdXe
        WHERE xe.IdCuaHang = @IdCuaHang
          AND d.TrangThaiDon = N'Hoàn thành'
          AND MONTH(d.NgayTraXe) = @Month
          AND YEAR(d.NgayTraXe) = @Year";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public int LaySoDonDaThue(int cuaHangId, int month, int year)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT COUNT(*)
        FROM DonThue
        WHERE IdCuaHang = @IdCuaHang
          AND TrangThaiDon = N'Hoàn thành'
          AND MONTH(NgayTraXe) = @Month
          AND YEAR(NgayTraXe) = @Year";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            return (int)command.ExecuteScalar();
        }
        public decimal LayDoanhThuThangNay(int cuaHangId, int month, int year)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT SUM(
            (
                CASE 
                    WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                    THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                    ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                END
            ) * (1 - ISNULL(km.mucGiamGia, 0))
        )
        FROM DonThue d
        JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
        JOIN Xe xe ON ctdt.IdXe = xe.IdXe
        LEFT JOIN KhuyenMai km ON d.IdKhuyenMai = km.IdKhuyenMai
        WHERE d.IdCuaHang = @IdCuaHang
          AND MONTH(d.ThoiGianTao) = @Month
          AND YEAR(d.ThoiGianTao) = @Year
          AND d.TrangThaiDon = N'Hoàn thành'";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            var result = command.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }

        // Doanh thu theo ngày trong tháng (chỉ tính đơn đã hoàn thành, có nhân khuyến mãi nếu có)
        public List<ThongKeTheoNgay> LayDoanhThuTheoNgay(int cuaHangId, int month, int year)
        {
            var result = new List<ThongKeTheoNgay>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        DECLARE @StartDate DATE = DATEFROMPARTS(@Year, @Month, 1);
        DECLARE @EndDate DATE = EOMONTH(@StartDate);

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
                    (
                        CASE 
                            WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                            THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                            ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                        END
                    ) * (1 - ISNULL(km.mucGiamGia, 0))
                ), 0) AS DoanhThu
            FROM DonThue d
            JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
            JOIN Xe xe ON ctdt.IdXe = xe.IdXe
            LEFT JOIN KhuyenMai km ON d.IdKhuyenMai = km.IdKhuyenMai
            WHERE d.IdCuaHang = @IdCuaHang
              AND d.ThoiGianTao >= @StartDate
              AND d.ThoiGianTao <= @EndDate
              AND d.TrangThaiDon = N'Hoàn thành'
            GROUP BY CAST(d.ThoiGianTao AS date)
        )
        SELECT 
            n.Ngay,
            ISNULL(dt.DoanhThu, 0) AS DoanhThu
        FROM NgayTrongKhoang n
        LEFT JOIN DoanhThuTheoNgay dt ON n.Ngay = dt.Ngay
        ORDER BY n.Ngay
        OPTION (MAXRECURSION 0);";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ThongKeTheoNgay
                {
                    Ngay = reader.GetDateTime(0).ToString("yyyy-MM-dd"),
                    DoanhThu = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1))
                });
            }
            return result;
        }

        public List<ThongKeLoaiXe> LayDoanhThuTheoLoaiXe(int cuaHangId, int month, int year)
        {
            var result = new List<ThongKeLoaiXe>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT lx.tenLoaiXe, 
            SUM(
                (
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) * (1 - ISNULL(km.mucGiamGia, 0))
            ) AS DoanhThu
        FROM DonThue d
        JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
        JOIN Xe xe ON ctdt.IdXe = xe.IdXe
        JOIN LoaiXe lx ON xe.idLoaiXe = lx.idLoaiXe
        LEFT JOIN KhuyenMai km ON d.IdKhuyenMai = km.IdKhuyenMai
        WHERE d.IdCuaHang = @IdCuaHang
          AND d.TrangThaiDon = N'Hoàn thành'
          AND MONTH(d.NgayTraXe) = @Month
          AND YEAR(d.NgayTraXe) = @Year
        GROUP BY lx.tenLoaiXe";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new ThongKeLoaiXe
                {
                    LoaiXe = reader.GetString(0),
                    DoanhThu = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1))
                });
            }
            return result;
        }

        // Doanh thu của từng xe (chỉ tính đơn đã hoàn thành, có nhân khuyến mãi nếu có)
        public List<DoanhThuXe> LayDoanhThuCacXe(int cuaHangId, int month, int year)
        {
            var result = new List<DoanhThuXe>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT xe.TenXe,
            SUM(
                (
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) * (1 - ISNULL(km.mucGiamGia, 0))
            ) AS DoanhThu
        FROM DonThue d
        JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
        JOIN Xe xe ON ctdt.IdXe = xe.IdXe
        LEFT JOIN KhuyenMai km ON d.IdKhuyenMai = km.IdKhuyenMai
        WHERE d.IdCuaHang = @IdCuaHang
          AND d.TrangThaiDon = N'Hoàn thành'
          AND MONTH(d.ThoiGianTao) = @Month
          AND YEAR(d.ThoiGianTao) = @Year
        GROUP BY xe.TenXe
        ORDER BY DoanhThu DESC";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new DoanhThuXe
                {
                    TenXe = reader.GetString(0),
                    DoanhThu = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1))
                });
            }
            return result;
        }

        // Top người dùng theo tổng tiền thuê (chỉ tính đơn đã hoàn thành, có nhân khuyến mãi nếu có)
        public List<TopNguoiDung> LayTopNguoiDung(int cuaHangId, int month, int year)
        {
            var result = new List<TopNguoiDung>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
        SELECT u.ho + ' ' + u.ten AS HoTen,
            SUM(
                (
                    CASE 
                        WHEN ISNULL(ctdt.GiaThueTheoGio, 0) = 0 
                        THEN ISNULL(ctdt.GiaThueTheoNgay, 0) * ctdt.SoLuong * (DATEDIFF(day, d.ngayNhanXe, d.ngayTraXe) + 1)
                        ELSE ISNULL(ctdt.GiaThueTheoGio, 0) * ctdt.SoLuong * DATEDIFF(hour, d.ngayNhanXe, d.ngayTraXe)
                    END
                ) * (1 - ISNULL(km.mucGiamGia, 0))
            ) AS TongTien
        FROM DonThue d
        JOIN ChiTietDonThue ctdt ON d.IdDonThue = ctdt.IdDonThue
        JOIN TaiKhoan u ON d.idTaiKhoan = u.idTaiKhoan
        LEFT JOIN KhuyenMai km ON d.IdKhuyenMai = km.IdKhuyenMai
        WHERE d.IdCuaHang = @IdCuaHang
          AND d.TrangThaiDon = N'Hoàn thành'
          AND MONTH(d.ThoiGianTao) = @Month
          AND YEAR(d.ThoiGianTao) = @Year
        GROUP BY u.ho, u.ten
        ORDER BY TongTien DESC
        OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY";
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@IdCuaHang", cuaHangId);
            command.Parameters.AddWithValue("@Month", month);
            command.Parameters.AddWithValue("@Year", year);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new TopNguoiDung
                {
                    TenNguoiDung = reader.GetString(0),
                    TongTienThue = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader.GetValue(1))
                });
            }
            return result;
        }
    }
}