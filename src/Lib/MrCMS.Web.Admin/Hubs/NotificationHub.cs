using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;

namespace MrCMS.Web.Admin.Hubs
{
    public class NotificationHub : Hub
    {
        public const string UsersGroup = "Users";
        public const string AdminGroup = "Admins";
        private readonly IGetCurrentClaimsPrincipal _userLookup;

        public NotificationHub(IGetCurrentClaimsPrincipal userLookup)
        {
            _userLookup = userLookup;
        }

        public override async Task OnConnectedAsync()
        {
            var user = await _userLookup.GetPrincipal();
            if (user == null || !user.IsAdmin())
                await Groups.AddToGroupAsync(Context.ConnectionId, UsersGroup);
            else
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userLookup.GetPrincipal();
            if (user == null || !user.IsAdmin())
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, UsersGroup);
            else
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
        }
    }
}