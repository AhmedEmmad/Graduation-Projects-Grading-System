using GradingManagementSystem.Core.Entities;
using Microsoft.AspNetCore.SignalR;

namespace GradingManagementSystem.APIs.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly string[] ValidRoles = { "Doctors", "Students", "All" };

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.IsInRole("Doctor") == true ? "Doctors" :
                       Context.User?.IsInRole("Student") == true ? "Students" : "All";

            if (role != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var role = Context.User?.IsInRole("Doctor") == true ? "Doctors" :
                       Context.User?.IsInRole("Student") == true ? "Students" : "All";

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
            if (!ValidRoles.Contains(role))
            {
                throw new HubException("Invalid recipient type. Must be 'Doctors', 'Students', or 'All'.");
            }
            string normalizedRole = role == NotificationRole.All.ToString() ? NotificationRole.All.ToString() :
                                   role == NotificationRole.Doctors.ToString() ? NotificationRole.Doctors.ToString()
                                   : NotificationRole.Students.ToString();

            if (normalizedRole == NotificationRole.All.ToString())
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
