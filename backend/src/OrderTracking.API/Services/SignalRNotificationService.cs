using Microsoft.AspNetCore.SignalR;
using OrderTracking.API.Hubs;
using OrderTracking.Application.Interfaces;

namespace OrderTracking.API.Services;

/// <summary>
/// Реализация INotificationService через SignalR.
/// Отправляет real-time уведомления подключённым клиентам.
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<OrderHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderNumber,
        string status,
        DateTime updatedAt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending SignalR notification: Order {OrderNumber} status changed to {Status}",
            orderNumber, status);

        await _hubContext.Clients.All.SendAsync(
            "OrderStatusChanged",
            new
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                Status = status,
                UpdatedAt = updatedAt
            },
            cancellationToken);
    }

    public async Task NotifyNewOrderCreatedAsync(
        Guid orderId,
        string orderNumber,
        string status,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending SignalR notification: New order {OrderNumber} created",
            orderNumber);

        await _hubContext.Clients.All.SendAsync(
            "NewOrderCreated",
            new
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                Status = status,
                CreatedAt = createdAt
            },
            cancellationToken);
    }
}