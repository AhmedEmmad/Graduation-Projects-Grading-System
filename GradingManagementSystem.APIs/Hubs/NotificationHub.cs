using Microsoft.AspNetCore.SignalR;

namespace GradingManagementSystem.APIs.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string title, string description, string role)
        {
            if (role.Equals("Doctors", StringComparison.OrdinalIgnoreCase))
                await Clients.Group("Doctors").SendAsync("ReceiveNotification", title, description, "Doctors");

            else if (role.Equals("Students", StringComparison.OrdinalIgnoreCase))
                await Clients.Group("Students").SendAsync("ReceiveNotification", title, description, "Students");

            else
                await Clients.All.SendAsync("ReceiveNotification", title, description, "All");
        }
        public async Task JoinGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }

        public async Task LeaveGroup(string role)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
        }
    }
}
