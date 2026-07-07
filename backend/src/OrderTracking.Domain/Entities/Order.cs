using System;
using OrderTracking.Domain.Enums;


namespace OrderTracking.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public string OrderNumber { get; private set; }
        public string Description { get; private set; }
        public OrderStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

#pragma warning disable CS8618
        private Order() { }
#pragma warning restore CS8618

        public Order(string description)
        {
            Id = Guid.NewGuid();
            OrderNumber = GenerateOrderNumber();
            Description = description ?? throw new ArgumentNullException(nameof(description));
            Status = OrderStatus.Created;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            if (Status == newStatus)
                return;
            if (!IsValidTransition(newStatus))
                throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }

        public static string GenerateOrderNumber()
        {
            var date = DateTime.UtcNow;
            var random = Random.Shared.Next(1000, 9999);
            return $"ORD-{date:yyyyMMdd}-{random}";
        }

        private bool IsValidTransition(OrderStatus newStatus)
        {
            return (Status, newStatus) switch
            {
                (OrderStatus.Created, OrderStatus.Sent) => true,
                (OrderStatus.Created, OrderStatus.Cancelled) => true,
                (OrderStatus.Sent, OrderStatus.Delivered) => true,
                (OrderStatus.Sent, OrderStatus.Cancelled) => true,
                (OrderStatus.Delivered, _) => false,
                (OrderStatus.Cancelled, _) => false,
                _ => false
            };
        }


    }
}
