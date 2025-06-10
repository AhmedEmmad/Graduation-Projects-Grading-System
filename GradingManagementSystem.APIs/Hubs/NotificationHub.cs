using GradingManagementSystem.Core.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace GradingManagementSystem.APIs.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly string[] ValidRoles = { "Doctors", "Students", "All" };

        // This method allows clients to join a specific group
        public async Task JoinGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || !ValidRoles.Contains(groupName))
            {
                throw new HubException("Invalid group name. Must be 'Doctors', 'Students', or 'All'.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "All");

                if (Context.User.IsInRole("Doctor"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Doctors");

                else if (Context.User.IsInRole("Student"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Students");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "All");

                if (Context.User.IsInRole("Doctor"))
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Doctors");

                else if (Context.User.IsInRole("Student"))
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Students");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotification(NotificationResponseDto model)
        {
            if (model is null || string.IsNullOrEmpty(model.Title)
                              || string.IsNullOrEmpty(model.Description)
                              || string.IsNullOrEmpty(model.Role))
            {
                throw new HubException("Invalid notification data.");
            }

            string normalizedRole = model.Role.Trim();
            if (!ValidRoles.Contains(normalizedRole))
            {
                throw new HubException("Invalid recipient type. Must be 'Doctors', 'Students', or 'All'.");
            }

            await Clients.Group(normalizedRole).SendAsync("ReceiveNotification", model);
        }
    }
}