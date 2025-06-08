using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ThueXeDapHoiAn.Areas.Client.ViewComponents
{
    public class ChatHubShop : Hub
    {
        public async Task SendMessage(string groupId, string senderName, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var time = System.DateTime.Now.ToString("HH:mm");  // Lấy giờ phút hiện tại

            await Clients.Group(groupId).SendAsync("ReceiveMessage", senderId, message, time);
        }


        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }
    }
}
