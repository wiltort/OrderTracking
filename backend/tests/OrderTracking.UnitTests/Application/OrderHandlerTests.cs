using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using OrderTracking.Application.Commands.CreateOrder;
using OrderTracking.Application.Commands.UpdateOrderStatus;
using OrderTracking.Application.DTOs;
using OrderTracking.Application.Events;
using OrderTracking.Application.Mappings;
using OrderTracking.Application.Queries.GetOrderById;
using OrderTracking.Application.Queries.GetOrders;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Enums;
using OrderTracking.Domain.Exceptions;
using OrderTracking.Domain.Interfaces;

namespace OrderTracking.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateOrderHandler(
            _orderRepositoryMock.Object,
            _eventPublisherMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrderAndPublishEvent()
    {
        // Arrange
        var description = "New test order";
        var command = new CreateOrderCommand(description);

        Order? capturedOrder = null;
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o)
            .ReturnsAsync((Order o) => o);

        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be(description);
        result.Status.Should().Be(OrderStatus.Created.ToString());

        _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldRethrowException()
    {
        // Arrange
        var command = new CreateOrderCommand("Test");
        _orderRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB error");
    }
}

public class UpdateOrderStatusHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<UpdateOrderStatusHandler>> _loggerMock;
    private readonly UpdateOrderStatusHandler _handler;

    public UpdateOrderStatusHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _handler = new UpdateOrderStatusHandler(
            _orderRepositoryMock.Object,
            _eventPublisherMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateStatusAndPublishEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order("Test order");
        var command = new UpdateOrderStatusCommand(orderId, "Sent");

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Sent.ToString());

        _orderRepositoryMock.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<OrderStatusChangedEvent>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new UpdateOrderStatusCommand(orderId, "Sent");

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OrderNotFoundException>()
            .WithMessage($"*{orderId}*");
    }

    [Fact]
    public async Task Handle_WithInvalidStatusTransition_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order("Test order");
        order.UpdateStatus(OrderStatus.Sent);
        order.UpdateStatus(OrderStatus.Delivered);
        var command = new UpdateOrderStatusCommand(orderId, "Cancelled");

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot transition from Delivered to Cancelled");
    }

    [Fact]
    public async Task Handle_WithInvalidStatusString_ShouldThrowArgumentException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order("Test order");
        var command = new UpdateOrderStatusCommand(orderId, "InvalidStatus");

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}

public class GetOrdersHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<GetOrdersHandler>> _loggerMock;
    private readonly GetOrdersHandler _handler;

    public GetOrdersHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<GetOrdersHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetOrdersHandler(
            _orderRepositoryMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrders_ShouldReturnAllOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order("Order 1"),
            new Order("Order 2"),
            new Order("Order 3")
        };

        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(orders);

        var query = new GetOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<OrderDto>();
    }

    [Fact]
    public async Task Handle_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        _orderRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Order>());

        var query = new GetOrdersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

public class GetOrderByIdHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<GetOrderByIdHandler>> _loggerMock;
    private readonly GetOrderByIdHandler _handler;

    public GetOrderByIdHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _loggerMock = new Mock<ILogger<GetOrderByIdHandler>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetOrderByIdHandler(
            _orderRepositoryMock.Object,
            _mapper,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order("Test order");
        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Test order");
        result.Status.Should().Be(OrderStatus.Created.ToString());
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderByIdQuery(orderId);

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<OrderNotFoundException>()
            .WithMessage($"*{orderId}*");
    }
}