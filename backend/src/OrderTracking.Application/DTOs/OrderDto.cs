using System;

namespace OrderTracking.Application.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public required string OrderNumber { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}