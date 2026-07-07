namespace OrderTracking.Application.Interfaces;

/// <summary>
/// Абстракция для отправки real-time уведомлений клиентам.
/// Реализация находится в API слое (SignalR).
/// </summary>
public interface INotificationService
{
    Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderNumber,
        string status,
        DateTime updatedAt,
        CancellationToken cancellationToken = default);

    Task NotifyNewOrderCreatedAsync(
        Guid orderId,
        string orderNumber,
        string status,
        DateTime createdAt,
        CancellationToken cancellationToken = default);
}