using System.Collections.Generic;
using MediatR;
using OrderTracking.Application.DTOs;

namespace OrderTracking.Application.Queries.GetOrders
{
    public class GetOrdersQuery : IRequest<IEnumerable<OrderDto>>
    {
    }
}