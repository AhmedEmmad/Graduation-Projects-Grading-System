using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using Microsoft.AspNetCore.Mvc;
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

            if (normalizedRole == NotificationRole.All.ToString())
            {
                await Clients.All.SendAsync("ReceiveNotification", model);
            }
            else
            {
                await Clients.Group(normalizedRole).SendAsync("ReceiveNotification", model);
            }
        }
    }
}
