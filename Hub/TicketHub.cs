using Microsoft.AspNetCore.SignalR;

namespace TicketTitan.Hubs
{
    public class TicketHub : Hub
    {

        public async Task SubscribeToHighPriorityAlerts()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "HighPriorityAlerts");
        }
    }
}
