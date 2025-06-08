using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ThueXeDapHoiAn.Areas.Client.Models;
using ThueXeDapHoiAn.Areas.Client.Models.ViewModels;
using ThueXeDapHoiAn.Data;

namespace ThueXeDapHoiAn.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client,Shop")]
    public class TinhNangController : Controller
    {
        private readonly AppDbContextClient _context;
        public TinhNangController(AppDbContextClient context)
        {
            _context = context;
        }

        [Route("Client/NhanTinClient")]
        [HttpGet]
        public IActionResult NhanTinClient(int? id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            // Lấy danh sách đoạn chat của user
            var dsDoanChat = _context.DoanChat
            .Include(dc => dc.CuaHang)
            .Where(dc => dc.IdTaiKhoan == userId)
            .Select(dc => new DoanChatViewModel
            {
                IdDoanChat = dc.IdDoanChat,
                CuaHang = new CuaHangModel_Client
                {
                    IdCuaHang = dc.CuaHang.IdCuaHang,
                    TenCuaHang = dc.CuaHang.TenCuaHang,
                    DiaChi = dc.CuaHang.DiaChi,
                    SoDienThoai = dc.CuaHang.SoDienThoai,
                    Gmail = dc.CuaHang.Gmail,
                    GioiThieu = dc.CuaHang.GioiThieu,
                    HinhAnh = dc.CuaHang.HinhAnh
                },
                IdTaiKhoanCuaHang = dc.CuaHang.IdTaiKhoan
            })
            .ToList();


            if (!dsDoanChat.Any())
            {
                // Nếu không có đoạn chat nào, trả về view với danh sách rỗng
                return View(new DanhSachVaChiTietChatViewModel
                {
                    DanhSachDoanChat = dsDoanChat,
                    ChiTietDoanChat = null
                });
            }

            // Nếu id không truyền hoặc id không thuộc dsDoanChat thì mặc định lấy đoạn chat đầu tiên
            int idDoanChat = id ?? dsDoanChat.First().IdDoanChat;
            if (!dsDoanChat.Any(dc => dc.IdDoanChat == idDoanChat))
                idDoanChat = dsDoanChat.First().IdDoanChat;

            // Lấy chi tiết đoạn chat đang active
            var doanChat = _context.DoanChat
                .Include(dc => dc.TinNhans)
                .Include(dc => dc.CuaHang)
                .Include(dc => dc.TaiKhoan)
                .FirstOrDefault(dc => dc.IdDoanChat == idDoanChat);

            var chiTietDoanChat = doanChat == null ? null : new DoanChatViewModel
            {
                IdDoanChat = doanChat.IdDoanChat,
                CuaHang = new CuaHangModel_Client
                {
                    IdCuaHang = doanChat.CuaHang.IdCuaHang,
                    TenCuaHang = doanChat.CuaHang.TenCuaHang,
                    DiaChi = doanChat.CuaHang.DiaChi,
                    SoDienThoai = doanChat.CuaHang.SoDienThoai,
                    Gmail = doanChat.CuaHang.Gmail,
                    GioiThieu = doanChat.CuaHang.GioiThieu,
                    HinhAnh = doanChat.CuaHang.HinhAnh
                },
                TenNguoiDung = doanChat.TaiKhoan.Ten,
                TinNhans = doanChat.TinNhans.OrderBy(t => t.ThoiGianGui).ToList(),
                IdTaiKhoanCuaHang = doanChat.CuaHang.IdTaiKhoan
            };


            // Trả về model tổng hợp chứa danh sách đoạn chat và chi tiết đoạn chat
            var vmTongHop = new DanhSachVaChiTietChatViewModel
            {
                DanhSachDoanChat = dsDoanChat,
                ChiTietDoanChat = chiTietDoanChat
            };

            return View(vmTongHop);
        }

        [HttpPost]
        public IActionResult GuiTinNhan(int idDoanChat, string tinNhanMoi)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var tn = new TinNhanModel
            {
                IdDoanChat = idDoanChat,
                IdTaiKhoanGui = userId,
                NoiDung = tinNhanMoi,
                ThoiGianGui = DateTime.Now
            };

            _context.TinNhan.Add(tn);
            _context.SaveChanges();

            return Ok(new { Success = true });
        }
        [HttpGet]
        public IActionResult DanhSachDoanChat()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            // Lấy danh sách đoạn chat của user
            var dsDoanChat = _context.DoanChat
                .Include(dc => dc.CuaHang) // Already included
                .Where(dc => dc.IdTaiKhoan == userId)
                .Select(dc => new DoanChatViewModel
                {
                    IdDoanChat = dc.IdDoanChat,
                    CuaHang = dc.CuaHang,
                })
                .ToList();


            return View(dsDoanChat);
        }

    }
}
