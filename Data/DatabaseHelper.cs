﻿using System.Data;
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
        public string GetTrangThaiCuaHang(int idTaiKhoan)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT trangThaiCuaHang FROM CuaHang WHERE idTaiKhoan = @id", conn);
                cmd.Parameters.AddWithValue("@id", idTaiKhoan);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return result.ToString(); // Trả về string, ví dụ: "1", "0", "true", "false", ...
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
                        Id = Convert.ToInt32(reader["idTaiKhoan"]),
                        Ho = reader["ho"].ToString(),
                        Ten = reader["ten"].ToString(),
                        SoDienThoai = reader["soDienThoai"].ToString(),
                        MatKhau = reader["matKhau"].ToString(),  // Raw password
                        HinhAnh = reader["hinhAnh"].ToString(),
                        VaiTro = reader["vaiTro"].ToString(),
                        TrangThai = Convert.ToBoolean(reader["trangThaiTaiKhoan"])
                    };
                }

                return null;
            }
        }
    }
}
