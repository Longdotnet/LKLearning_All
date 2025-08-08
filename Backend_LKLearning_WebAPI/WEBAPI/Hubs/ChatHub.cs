using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WEBAPI.Services;

namespace WEBAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IOnlineUserTracker _tracker;

        public ChatHub(IOnlineUserTracker tracker)
        {
            _tracker = tracker;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out int id))
                _tracker.SetOnline(id, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userId, out int id))
                _tracker.SetOffline(id);
            return base.OnDisconnectedAsync(exception);
        }

        // Sửa lại method này:
        public async Task SendMessage(int conversationId, int senderId, string senderName, string content, string sentAt)
        {
            await Clients.Group($"Conversation_{conversationId}")
                .SendAsync("ReceiveMessage", senderId, senderName, content, sentAt);
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }
    }
}
