using System;
using MediatR;
using OrderTracking.Application.DTOs;

namespace OrderTracking.Application.Queries.GetOrderById
{
    public class GetOrderByIdQuery(Guid id) : IRequest<OrderDto>
    {
        public Guid Id { get; set; } = id;
    }
}