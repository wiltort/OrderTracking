using System;

namespace OrderTracking.Domain.Exceptions
{
    public class OrderNotFoundException : DomainException
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

        public override int StatusCode => 404;

        public override string ErrorCode => "ORDER_NOT_FOUND";
    }
}
