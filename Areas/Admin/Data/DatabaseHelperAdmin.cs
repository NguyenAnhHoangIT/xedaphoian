using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Areas.Admin.Models;
using ThueXeDapHoiAn.Areas.Admin.ViewModels;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelperAdmin
    {
        private readonly string _connectionString;

        public DatabaseHelperAdmin(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }
        public List<TaiKhoanModel> GetAllUsers()
        {
            var users = new List<TaiKhoanModel>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM TaiKhoan", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new TaiKhoanModel
                    {
                        IdTaiKhoan = Convert.ToInt32(reader["idTaiKhoan"]),
                        Ho = reader["ho"].ToString(),
                        Ten = reader["ten"].ToString(),
                        SoDienThoai = reader["soDienThoai"].ToString(),
                        MatKhau = reader["matKhau"].ToString(),
                        HinhAnh = reader["hinhAnh"].ToString(),
                        VaiTro = reader["vaiTro"].ToString(),
                        TrangThaiTaiKhoan = Convert.ToBoolean(reader["trangThaiTaiKhoan"])
                    });
                }
            }
            return users;
        }
        public bool UpdateUser(TaiKhoanModel user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"UPDATE TaiKhoan
                      SET ho = @Ho,
                          ten = @Ten,
                          soDienThoai = @SoDienThoai,
                          matKhau = @MatKhau,
                          vaiTro = @VaiTro,
                          trangThaiTaiKhoan = @TrangThaiTaiKhoan
                      WHERE idTaiKhoan = @IdTaiKhoan";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdTaiKhoan", user.IdTaiKhoan);
                    command.Parameters.AddWithValue("@Ho", user.Ho ?? "");
                    command.Parameters.AddWithValue("@Ten", user.Ten ?? "");
                    command.Parameters.AddWithValue("@SoDienThoai", user.SoDienThoai ?? "");
                    command.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? "");
                    command.Parameters.AddWithValue("@VaiTro", user.VaiTro ?? "");
                    command.Parameters.AddWithValue("@TrangThaiTaiKhoan", user.TrangThaiTaiKhoan);

                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
        public bool InsertUser(TaiKhoanModel user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"INSERT INTO TaiKhoan (ho, ten, soDienThoai, matKhau, vaiTro, trangThaiTaiKhoan, hinhAnh)
                      VALUES (@Ho, @Ten, @SoDienThoai, @MatKhau, @VaiTro, @TrangThaiTaiKhoan, @HinhAnh)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ho", user.Ho ?? "");
                    command.Parameters.AddWithValue("@Ten", user.Ten ?? "");
                    command.Parameters.AddWithValue("@SoDienThoai", user.SoDienThoai ?? "");
                    command.Parameters.AddWithValue("@MatKhau", user.MatKhau ?? "");
                    command.Parameters.AddWithValue("@VaiTro", user.VaiTro ?? "Client");
                    command.Parameters.AddWithValue("@TrangThaiTaiKhoan", user.TrangThaiTaiKhoan);
                    command.Parameters.AddWithValue("@HinhAnh", user.HinhAnh ?? "default.png");

                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
        public List<CuaHangViewModel> GetStoresWithBikes()
        {
            var cuaHangList = new List<CuaHangViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query joining CuaHang and Xe
                var query = @"
                SELECT ch.idCuaHang, ch.tenCuaHang, ch.diaChi, ch.soDienThoai,
                       xe.idXe, xe.tenXe, xe.giaThueTheoGio, xe.giaThueTheoNgay, xe.soLuong,
                       xe.gioiThieu, xe.hinhAnh, xe.trangThaiLoaiXe
                FROM CuaHang ch
                LEFT JOIN Xe xe ON ch.idCuaHang = xe.idCuaHang AND xe.trangThaiLoaiXe = 1
                WHERE ch.trangThaiCuaHang = 'True'
                ORDER BY ch.idCuaHang";

                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();

                CuaHangViewModel currentStore = null;
                int currentStoreId = -1;

                while (reader.Read())
                {
                    int storeId = Convert.ToInt32(reader["idCuaHang"]);

                    if (currentStore == null || storeId != currentStoreId)
                    {
                        currentStore = new CuaHangViewModel
                        {
                            Id = storeId,
                            TenCuaHang = reader["tenCuaHang"].ToString(),
                            DiaChi = reader["diaChi"].ToString(),
                            SoDienThoai = reader["soDienThoai"].ToString(),
                            XeList = new List<XeViewModel>()
                        };
                        cuaHangList.Add(currentStore);
                        currentStoreId = storeId;
                    }

                    if (reader["idXe"] != DBNull.Value) // If there is a bike
                    {
                        currentStore.XeList.Add(new XeViewModel
                        {
                            Id = Convert.ToInt32(reader["idXe"]),
                            TenXe = reader["tenXe"].ToString(),
                            GiaThueTheoGio = Convert.ToDecimal(reader["giaThueTheoGio"]),
                            GiaThueTheoNgay = Convert.ToDecimal(reader["giaThueTheoNgay"]),
                            SoLuong = Convert.ToInt32(reader["soLuong"]),
                            GioiThieu = reader["gioiThieu"].ToString(),
                            HinhAnh = reader["hinhAnh"].ToString(),
                            TrangThai = Convert.ToBoolean(reader["trangThaiLoaiXe"])
                        });
                    }
                }
            }

            return cuaHangList;
        }
        public bool ChangeTrangThaiXe(int idXe)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "UPDATE Xe SET trangThaiLoaiXe = 0 WHERE idXe = @IdXe";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdXe", idXe);
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }
        public TaiKhoanModel GetUserById(int id)
        {
            TaiKhoanModel user = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT * FROM TaiKhoan WHERE idTaiKhoan = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        user = new TaiKhoanModel
                        {
                            IdTaiKhoan = Convert.ToInt32(reader["idTaiKhoan"]),
                            Ho = reader["ho"].ToString(),
                            Ten = reader["ten"].ToString(),
                            SoDienThoai = reader["soDienThoai"].ToString(),
                            MatKhau = reader["matKhau"].ToString(),
                            HinhAnh = reader["hinhAnh"].ToString(),
                            VaiTro = reader["vaiTro"].ToString(),
                            TrangThaiTaiKhoan = Convert.ToBoolean(reader["trangThaiTaiKhoan"])
                        };
                    }
                }
            }

            return user;
        }
        public List<CuaHangTaiKhoanViewModel> GetPendingCuaHang()
        {
            var pendingStores = new List<CuaHangTaiKhoanViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                SELECT ch.idCuaHang, ch.tenCuaHang, ch.diaChi, ch.soDienThoai, 
                ch.gmail, ch.gioiThieu, ch.hinhAnh, ch.trangThaiCuaHang,
                tk.ho, tk.ten, tk.soDienThoai AS TaiKhoanSDT, tk.idTaiKhoan AS idTaiKhoan
                FROM CuaHang ch
                LEFT JOIN TaiKhoan tk ON ch.idTaiKhoan = tk.idTaiKhoan
                WHERE ch.trangThaiCuaHang = 'Pending'";

                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        pendingStores.Add(new CuaHangTaiKhoanViewModel
                        {
                            Id = Convert.ToInt32(reader["idCuaHang"]),
                            TenCuaHang = reader["tenCuaHang"].ToString(),
                            DiaChi = reader["diaChi"].ToString(),
                            SoDienThoai = reader["soDienThoai"].ToString(),
                            Gmail = reader["gmail"].ToString(),
                            GioiThieu = reader["gioiThieu"].ToString(),
                            HinhAnh = reader["hinhAnh"].ToString(),
                            TrangThai = reader["trangThaiCuaHang"].ToString(),
                            Ho = reader["ho"].ToString(),
                            Ten = reader["ten"].ToString(),
                            TaiKhoanSDT = reader["TaiKhoanSDT"].ToString(),
                            IdTaiKhoan = Convert.ToInt32(reader["idTaiKhoan"])
                        });
                    }
                }
            }
            return pendingStores;
        }

        public bool ChapNhanCuaHang(int idCuaHang)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "UPDATE CuaHang SET trangThaiCuaHang = 'True' WHERE idCuaHang = @IdCuaHang";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCuaHang", idCuaHang);
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        public bool TuChoiCuaHang(int idCuaHang)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM CuaHang WHERE idCuaHang = @IdCuaHang";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@IdCuaHang", idCuaHang);
                    connection.Open();
                    int rows = command.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        public int GetUserCount()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM TaiKhoan";
            using var command = new SqlCommand(query, connection);
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public int GetVehicleCount()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM Xe";
            using var command = new SqlCommand(query, connection);
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public int GetStoreCount()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "SELECT COUNT(*) FROM CuaHang WHERE trangThaiCuaHang = 'True'";
            using var command = new SqlCommand(query, connection);
            connection.Open();
            return (int)command.ExecuteScalar();
        }

        public decimal GetMonthlyRevenue()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT ISNULL(SUM(
                c.soLuong * 
                CASE 
                    WHEN ISNULL(c.giaThueTheoGio, 0) > 0 
                        THEN c.giaThueTheoGio * DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe)
                    ELSE c.giaThueTheoNgay * 
                        CASE WHEN DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) = 0 THEN 1 ELSE DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) END
                END
            ), 0)
            FROM ChiTietDonThue c
            JOIN DonThue d ON c.idDonThue = d.idDonThue
            WHERE YEAR(d.thoiGianTao) = YEAR(GETDATE())
              AND MONTH(d.thoiGianTao) = MONTH(GETDATE())";
            using var command = new SqlCommand(query, connection);
            connection.Open();
            return (decimal)command.ExecuteScalar();
        }



        // Lấy top 10 người dùng theo tổng tiền thuê
        public List<UserTopModel> GetTopUsersByRentalAmount(int top = 10)
        {
            var list = new List<UserTopModel>();
            using var connection = new SqlConnection(_connectionString);

            var query = $@"
            SELECT TOP ({top}) tk.ten, SUM(
                DATEDIFF(DAY, dt.ngayNhanXe, dt.ngayTraXe) * ct.soLuong * ISNULL(ct.giaThueTheoNgay, 0)
            ) AS TongTien
            FROM DonThue dt
            INNER JOIN TaiKhoan tk ON dt.idTaiKhoan = tk.idTaiKhoan
            INNER JOIN ChiTietDonThue ct ON dt.idDonThue = ct.idDonThue
            GROUP BY tk.ten
            ORDER BY TongTien DESC";

            using var command = new SqlCommand(query, connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new UserTopModel
                {
                    TenNguoiDung = reader["ten"].ToString(),
                    TongTienThue = Convert.ToDecimal(reader["TongTien"])
                });
            }
            return list;
        }



        public List<StoreRevenueModel> GetStoreRevenueCurrentMonth()
        {
            var rawList = new List<StoreRevenueModel>();
            using var connection = new SqlConnection(_connectionString);

            var query = @"
            SELECT ch.tenCuaHang, 
                   CONVERT(VARCHAR, CAST(d.thoiGianTao AS DATE), 103) AS Ngay,
                   ISNULL(SUM(
                       c.soLuong * 
                       CASE 
                           WHEN ISNULL(c.giaThueTheoGio, 0) > 0 
                               THEN c.giaThueTheoGio * 
                                    CASE WHEN DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) = 0 THEN 1 ELSE DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) END
                           ELSE c.giaThueTheoNgay * 
                               CASE WHEN DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) = 0 THEN 1 ELSE DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) END
                       END
                   ), 0) AS DoanhThu
            FROM CuaHang ch
            LEFT JOIN DonThue d ON ch.idCuaHang = d.idCuaHang 
                AND YEAR(d.thoiGianTao) = YEAR(GETDATE())
                AND MONTH(d.thoiGianTao) = MONTH(GETDATE())
            LEFT JOIN ChiTietDonThue c ON d.idDonThue = c.idDonThue
            GROUP BY ch.tenCuaHang, CAST(d.thoiGianTao AS DATE)
            ORDER BY ch.tenCuaHang, CAST(d.thoiGianTao AS DATE)";

            using var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    rawList.Add(new StoreRevenueModel
                    {
                        TenCuaHang = reader["tenCuaHang"]?.ToString() ?? "Unknown",
                        Ngay = reader["Ngay"].ToString(), // "dd/MM/yyyy"
                        DoanhThu = Convert.ToDecimal(reader["DoanhThu"])
                    });
                }
            }
            catch (SqlException ex)
            {
                // Log the exception (e.g., using a logging framework)
                Console.WriteLine($"SQL Error in GetStoreRevenueCurrentMonth: {ex.Message}");
                return new List<StoreRevenueModel>(); // Return empty list on error
            }

            // Generate all days in the current month up to today
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = Enumerable.Range(0, (today - firstDayOfMonth).Days + 1)
                .Select(i => firstDayOfMonth.AddDays(i).ToString("dd/MM/yyyy"))
                .ToList();

            var result = new List<StoreRevenueModel>();
            var stores = rawList.Select(x => x.TenCuaHang).Distinct();

            foreach (var store in stores)
            {
                var storeData = rawList.Where(x => x.TenCuaHang == store)
                                       .ToDictionary(x => x.Ngay, x => x.DoanhThu);

                foreach (var day in daysInMonth)
                {
                    result.Add(new StoreRevenueModel
                    {
                        TenCuaHang = store,
                        Ngay = day,
                        DoanhThu = storeData.ContainsKey(day) ? storeData[day] : 0
                    });
                }
            }

            return result;
        }




        // Top 5 cửa hàng theo doanh thu
        public List<StoreTopModel> GetTopStoresByRevenue(int top = 5)
        {
            var list = new List<StoreTopModel>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT TOP (@Top) ch.tenCuaHang, 
                   ISNULL(SUM(
                       c.soLuong * 
                       CASE 
                           WHEN ISNULL(c.giaThueTheoGio, 0) > 0 
                               THEN c.giaThueTheoGio * 
                                    CASE WHEN DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) = 0 
                                         THEN 1 
                                         ELSE DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) 
                                    END
                           ELSE c.giaThueTheoNgay * 
                                CASE WHEN DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) = 0 
                                     THEN 1 
                                     ELSE DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) 
                                END
                       END
                   ), 0) AS DoanhThu
            FROM CuaHang ch
            LEFT JOIN DonThue d ON ch.idCuaHang = d.idCuaHang
                AND YEAR(d.thoiGianTao) = YEAR(GETDATE())
                AND MONTH(d.thoiGianTao) = MONTH(GETDATE())
            LEFT JOIN ChiTietDonThue c ON d.idDonThue = c.idDonThue
            GROUP BY ch.tenCuaHang
            ORDER BY DoanhThu DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Top", top);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                int rank = 1;
                while (reader.Read())
                {
                    list.Add(new StoreTopModel
                    {
                        TenCuaHang = reader["tenCuaHang"]?.ToString() ?? "Unknown",
                        DoanhThu = Convert.ToDecimal(reader["DoanhThu"]),
                        Hang = rank++
                    });
                }
            }
            catch (SqlException ex)
            {
                // Log the exception (e.g., using a logging framework)
                Console.WriteLine($"SQL Error in GetTopStoresByRevenue: {ex.Message}");
                return list; // Return empty list or handle as needed
            }

            return list;
        }


        // Xếp hạng xe theo số lượng thuê
        public List<VehicleRankingModel> GetVehicleRanking(int top = 10)
        {
            var list = new List<VehicleRankingModel>();
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT TOP (@Top) x.tenXe, COUNT(ct.idXe) AS SoLuong
            FROM ChiTietDonThue ct
            INNER JOIN DonThue d ON ct.idDonThue = d.idDonThue
            INNER JOIN Xe x ON ct.idXe = x.idXe
            WHERE YEAR(d.thoiGianTao) = YEAR(GETDATE())
              AND MONTH(d.thoiGianTao) = MONTH(GETDATE())
            GROUP BY x.tenXe, x.idXe
            ORDER BY SoLuong DESC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Top", top);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                int rank = 1;
                while (reader.Read())
                {
                    list.Add(new VehicleRankingModel
                    {
                        TenXe = reader["tenXe"]?.ToString() ?? "Unknown",
                        SoLuong = Convert.ToInt32(reader["SoLuong"]),
                        Hang = rank++
                    });
                }
            }
            catch (SqlException ex)
            {
                // Log the exception
                Console.WriteLine($"SQL Error in GetVehicleRanking: {ex.Message}");
                return list;
            }

            return list;
        }

        public List<VehicleRevenueModel> GetVehicleRevenue()
        {
            var list = new List<VehicleRevenueModel>();
            var colors = new[] { "#667EEA", "#764BA2", "#FF6B6B", "#FFA931", "#4ECDC4" };
            using var connection = new SqlConnection(_connectionString);
            var query = @"
            SELECT 
                lx.tenLoaiXe AS TenXe,
                ISNULL(SUM(
                    c.soLuong * 
                    CASE 
                        WHEN ISNULL(c.giaThueTheoGio, 0) > 0 
                            THEN c.giaThueTheoGio * 
                                 CASE WHEN DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) = 0 
                                      THEN 1 
                                      ELSE DATEDIFF(HOUR, d.ngayNhanXe, d.ngayTraXe) 
                                 END
                        ELSE c.giaThueTheoNgay * 
                             CASE WHEN DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) = 0 
                                  THEN 1 
                                  ELSE DATEDIFF(DAY, d.ngayNhanXe, d.ngayTraXe) 
                             END
                    END
                ), 0) AS DoanhThu
            FROM LoaiXe lx
            LEFT JOIN Xe x ON lx.idLoaiXe = x.idLoaiXe
            LEFT JOIN ChiTietDonThue c ON x.idXe = c.idXe
            LEFT JOIN DonThue d ON c.idDonThue = d.idDonThue
                AND YEAR(d.thoiGianTao) = YEAR(GETDATE())
                AND MONTH(d.thoiGianTao) = MONTH(GETDATE())
            GROUP BY lx.tenLoaiXe
            ORDER BY DoanhThu DESC";

            using var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                int index = 0;
                while (reader.Read())
                {
                    list.Add(new VehicleRevenueModel
                    {
                        TenXe = reader["TenXe"]?.ToString() ?? "Unknown",
                        DoanhThu = Convert.ToDecimal(reader["DoanhThu"]),
                        Color = colors[index % colors.Length] // Assign color
                    });
                    index++;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in GetVehicleRevenue: {ex.Message}");
                return list;
            }

            return list;
        }

    }
}
