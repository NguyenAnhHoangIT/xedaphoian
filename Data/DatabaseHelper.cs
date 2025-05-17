using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Models;

namespace ThueXeDapHoiAn.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default");
        }
        public bool HasCuaHang(int idTaiKhoan)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT COUNT(*) FROM CuaHang WHERE idTaiKhoan = @id", conn);
                cmd.Parameters.AddWithValue("@id", idTaiKhoan);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        public int? GetTrangThaiCuaHang(int idTaiKhoan)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT trangThaiCuaHang FROM CuaHang WHERE idTaiKhoan = @id", conn);
                cmd.Parameters.AddWithValue("@id", idTaiKhoan);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result); // Trạng thái: 1 hoặc 0
                }

                return null; // Không có cửa hàng
            }
        }


        public UserModel GetUserByPhoneNumber(string phoneNumber)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("SELECT * FROM TaiKhoan WHERE SoDienThoai = @PhoneNumber", connection);
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                connection.Open();
                var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new UserModel
                    {
                        Id = reader["idTaiKhoan"].ToString(),
                        Ho = reader["ho"].ToString(),
                        Ten = reader["ten"].ToString(),
                        SoDienThoai = reader["soDienThoai"].ToString(),
                        MatKhau = reader["matKhau"].ToString(),  // Raw password
                        HinhAnh = reader["hinhAnh"].ToString(),
                        VaiTro = reader["vaiTro"].ToString(),
                        TrangThai = reader["trangThaiTaiKhoan"].ToString()
                    };
                }

                return null;
            }
        }
    }
}
