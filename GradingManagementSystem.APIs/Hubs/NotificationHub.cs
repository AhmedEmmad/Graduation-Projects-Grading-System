using Microsoft.AspNetCore.SignalR;

namespace GradingManagementSystem.APIs.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly string[] ValidRoles = { "Doctors", "Students", "All" };
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
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(role))
            {
                throw new HubException("Title, Description, and Role cannot be empty.");
            }

            role = role.Trim();
            if (!ValidRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                throw new HubException("Invalid recipient type. Must be 'Doctors', 'Students', or 'All'.");
            }
            string normalizedRole = role.Equals("All", StringComparison.OrdinalIgnoreCase) ? "All" :
                                   role.Equals("Doctors", StringComparison.OrdinalIgnoreCase) ? "Doctors" : "Students";

            if (normalizedRole == "All")
            {
                await Clients.All.SendAsync("ReceiveNotification", title, description, normalizedRole);
            }
            else
            {
                await Clients.Group(normalizedRole).SendAsync("ReceiveNotification", title, description, normalizedRole);
            }
        }
    }
}
