using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
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
                        Id = Convert.ToInt32(reader["idTaiKhoan"]),
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
