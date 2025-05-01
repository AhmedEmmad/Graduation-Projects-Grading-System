using Microsoft.AspNetCore.SignalR;

namespace GradingManagementSystem.APIs.Hubs
{
    public class NotificationHub : Hub
    {

        public override async Task OnConnectedAsync()
        {
            var role = Context.User.IsInRole("Doctor") ? "Doctors" :
                       Context.User.IsInRole("Student") ? "Students" : null;

            if (role != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var role = Context.User.IsInRole("Doctor") ? "Doctors" :
                       Context.User.IsInRole("Student") ? "Students" : null;

            if (role != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(string title, string description, string role)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                throw new HubException("Title and description cannot be empty.");
            }

            if (role.Equals("Doctors", StringComparison.OrdinalIgnoreCase))
                await Clients.Group("Doctors").SendAsync("ReceiveNotification", title, description, "Doctors");
            else if (role.Equals("Students", StringComparison.OrdinalIgnoreCase))
                await Clients.Group("Students").SendAsync("ReceiveNotification", title, description, "Students");
            else
                await Clients.All.SendAsync("ReceiveNotification", title, description, "All");
        }
    }
}
