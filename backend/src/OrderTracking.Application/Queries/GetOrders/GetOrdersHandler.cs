using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.DTOs;
using OrderTracking.Domain.Interfaces;

namespace OrderTracking.Application.Queries.GetOrders
{
    public class GetOrdersHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<GetOrdersHandler> logger
    ) : IRequestHandler<GetOrdersQuery, IEnumerable<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetOrdersHandler> _logger = logger;

        public async Task<IEnumerable<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all orders");
            
            var orders = await _orderRepository.GetAllAsync();
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
            
            _logger.LogInformation("Retrieved {Count} orders", orderDtos.Count());
            
            return orderDtos;
        }
    }
}