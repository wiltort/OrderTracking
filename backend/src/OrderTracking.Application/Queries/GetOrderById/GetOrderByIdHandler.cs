using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.DTOs;
using OrderTracking.Domain.Exceptions;
using OrderTracking.Domain.Interfaces;

namespace OrderTracking.Application.Queries.GetOrderById
{
    public class GetOrderByIdHandler(
        IOrderRepository orderRepository,
        IMapper mapper,
        ILogger<GetOrderByIdHandler> logger
    ) : IRequestHandler<GetOrderByIdQuery, OrderDto>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetOrderByIdHandler> _logger = logger;

        public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting order with ID: {OrderId}", request.Id);
            
            var order = await _orderRepository.GetByIdAsync(request.Id);
            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", request.Id);
                throw new OrderNotFoundException(request.Id);
            }
            
            return _mapper.Map<OrderDto>(order);
        }
    }
}