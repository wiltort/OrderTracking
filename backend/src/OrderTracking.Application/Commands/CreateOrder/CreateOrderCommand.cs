using MediatR;
using OrderTracking.Application.DTOs;

namespace OrderTracking.Application.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<OrderDto>
    {
        public string Description { get; set; }

        public CreateOrderCommand(string description)
        {
            Description = description;
        }
    }
}