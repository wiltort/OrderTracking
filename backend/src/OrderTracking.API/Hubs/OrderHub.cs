using Microsoft.AspNetCore.SignalR;

namespace OrderTracking.API.Hubs
{
    public class OrderHub : Hub
    {
        private readonly ILogger<OrderHub> _logger;

        public OrderHub(ILogger<OrderHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await Clients.Client(Context.ConnectionId).SendAsync(
                "Connected",
                new
                {
                    Message = "Connected to order tracking hub",
                    ConnectionId = Context.ConnectionId,
                    Timestamp = DateTime.UtcNow
                });
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToOrder(Guid orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order-{orderId}");
            _logger.LogInformation("Client {ConnectionId} subscribed to order {OrderId}", 
                Context.ConnectionId, orderId);
            await Clients.Client(Context.ConnectionId).SendAsync(
                "Subscribed",
                new
                {
                    Message = $"Subscribed to order {orderId}",
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow
                });
        }

        public async Task UnsubscribeFromOrder(Guid orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order-{orderId}");
            _logger.LogInformation("Client {ConnectionId} unsubscribed from order {OrderId}", 
                Context.ConnectionId, orderId);
            await Clients.Client(Context.ConnectionId).SendAsync(
                "Unsubscribed",
                new
                {
                    Message = $"Unsubscribed from order {orderId}",
                    OrderId = orderId,
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}