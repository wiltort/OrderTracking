using System.Threading.Tasks;

namespace OrderTracking.Domain.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T @event, string? topic = null);
    }
}