using System;

namespace OrderTracking.Domain.Exceptions
{
    public class OrderNotFoundException : Exception
    {
        public Guid OrderId { get; }

        public OrderNotFoundException(Guid orderId)
            : base($"Order with ID {orderId} not found")
        {
            OrderId = orderId;
        }

        public OrderNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
