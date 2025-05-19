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
                WHERE ch.trangThaiCuaHang = 1
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

    }
}
