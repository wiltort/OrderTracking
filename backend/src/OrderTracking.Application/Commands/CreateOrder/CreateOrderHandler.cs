using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.DTOs;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Application.Events;

namespace OrderTracking.Application.Commands.CreateOrder
{
    public class CreateOrderHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<CreateOrderHandler> logger) : IRequestHandler<CreateOrderCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IEventPublisher _eventPublisher = eventPublisher;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CreateOrderHandler> _logger = logger;

        public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creating new order with description: {Description}", request.Description);

                var order = new Order(request.Description);
                var createdOrder = await _orderRepository.AddAsync(order);

                await _eventPublisher.PublishAsync(new OrderCreatedEvent
                {
                    OrderId = createdOrder.Id,
                    OrderNumber = createdOrder.OrderNumber,
                    Description = createdOrder.Description,
                    Status = createdOrder.Status.ToString(),
                    CreatedAt = createdOrder.CreatedAt
                });

                _logger.LogInformation("Order created successfully with ID: {OrderId}", createdOrder.Id);

                return _mapper.Map<OrderDto>(createdOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order with description: {Description}", request.Description);
                throw;
            }
        }
    }
}