using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderTracking.Application.DTOs;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Enums;
using OrderTracking.Domain.Interfaces;
using OrderTracking.Application.Events;

namespace OrderTracking.Application.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        IMapper mapper,
        ILogger<UpdateOrderStatusHandler> logger
    ) : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IEventPublisher _eventPublisher = eventPublisher;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateOrderStatusHandler> _logger = logger;

        public async Task<OrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating status of order {OrderId} to {Status}", 
                    request.OrderId, request.Status);

                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                    throw new InvalidOperationException($"Order with ID {request.OrderId} not found");
                }

                var newStatus = Enum.Parse<OrderStatus>(request.Status);
                order.UpdateStatus(newStatus);

                var updatedOrder = await _orderRepository.UpdateAsync(order);

                await _eventPublisher.PublishAsync(new OrderStatusChangedEvent
                {
                    OrderId = updatedOrder.Id,
                    OrderNumber = updatedOrder.OrderNumber,
                    OldStatus = order.Status.ToString(),
                    NewStatus = updatedOrder.Status.ToString(),
                    UpdatedAt = updatedOrder.UpdatedAt
                });

                _logger.LogInformation("Order {OrderId} status updated to {Status}", 
                    request.OrderId, request.Status);

                return _mapper.Map<OrderDto>(updatedOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of order {OrderId}", request.OrderId);
                throw;
            }
        }
    }
}