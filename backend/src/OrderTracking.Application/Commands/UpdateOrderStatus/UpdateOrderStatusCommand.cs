using System;
using MediatR;
using OrderTracking.Application.DTOs;

namespace OrderTracking.Application.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommand : IRequest<OrderDto>
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; }

        public UpdateOrderStatusCommand(Guid orderId, string status)
        {
            OrderId = orderId;
            Status = status;
        }
    }
}