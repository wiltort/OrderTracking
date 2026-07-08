using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using OrderTracking.API.Controllers;
using OrderTracking.Application.Commands.CreateOrder;
using OrderTracking.Application.Commands.UpdateOrderStatus;
using OrderTracking.Application.DTOs;
using OrderTracking.Application.Queries.GetOrderById;
using OrderTracking.Application.Queries.GetOrders;
using OrderTracking.Domain.Exceptions;

namespace OrderTracking.UnitTests.API;

public class OrdersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<OrdersController>> _loggerMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<OrdersController>>();
        _controller = new OrdersController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrders_ShouldReturnOkWithOrders()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            new() { Id = Guid.NewGuid(), OrderNumber = "ORD-001", Description = "Test 1", Status = "Created", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), OrderNumber = "ORD-002", Description = "Test 2", Status = "Sent", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetOrders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
        returnedOrders.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrders_WhenEmpty_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var result = await _controller.GetOrders();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrders = okResult.Value.Should().BeAssignableTo<IEnumerable<OrderDto>>().Subject;
        returnedOrders.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrder_WithExistingId_ShouldReturnOkWithOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            Description = "Test order",
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.Id == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.Id.Should().Be(orderId);
    }

    [Fact]
    public async Task GetOrder_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateOrderRequest { Description = "New order" };
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Description = "New order",
            Status = "Created",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrdersController.GetOrder));
        createdResult.RouteValues!["id"].Should().Be(orderDto.Id);
        createdResult.Value.Should().Be(orderDto);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = "Sent" };
        var orderDto = new OrderDto
        {
            Id = orderId,
            OrderNumber = "ORD-001",
            Description = "Test",
            Status = "Sent",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedOrder = okResult.Value.Should().BeOfType<OrderDto>().Subject;
        returnedOrder.Status.Should().Be("Sent");
    }

    [Fact]
    public async Task UpdateOrderStatus_WithNonExistentOrder_ShouldThrowOrderNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest { Status = "Sent" };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateOrderStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OrderNotFoundException(orderId));

        // Act
        var act = () => _controller.UpdateOrderStatus(orderId, request);

        // Assert
        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}