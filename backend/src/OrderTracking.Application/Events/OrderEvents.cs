using System;

namespace OrderTracking.Application.Events
{
    public class OrderStatusChangedEvent
    {
        public Guid OrderId { get; set; }
        public required string OrderNumber { get; set; }
        public required string OldStatus { get; set; }
        public required string NewStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderCreatedEvent
    {
        public Guid OrderId { get; set; }
        public required string OrderNumber { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}