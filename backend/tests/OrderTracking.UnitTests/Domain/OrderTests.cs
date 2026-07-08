using FluentAssertions;
using OrderTracking.Domain.Entities;
using OrderTracking.Domain.Enums;

namespace OrderTracking.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidDescription_ShouldSetProperties()
    {
        // Arrange
        var description = "Test order description";

        // Act
        var order = new Order(description);

        // Assert
        order.Id.Should().NotBeEmpty();
        order.Description.Should().Be(description);
        order.Status.Should().Be(OrderStatus.Created);
        order.OrderNumber.Should().NotBeNullOrWhiteSpace();
        order.OrderNumber.Should().StartWith("ORD-");
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateOrder_WithNullDescription_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new Order(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("description");
    }

    [Fact]
    public void CreateOrder_WithEmptyDescription_ShouldCreateOrder()
    {
        // Arrange
        var description = string.Empty;

        // Act
        var order = new Order(description);

        // Assert
        order.Description.Should().BeEmpty();
    }

    [Fact]
    public void GenerateOrderNumber_ShouldHaveCorrectFormat()
    {
        // Act
        var orderNumber = Order.GenerateOrderNumber();

        // Assert
        orderNumber.Should().MatchRegex(@"^ORD-\d{8}-\d{4}$");
    }

    [Fact]
    public void UpdateStatus_FromCreatedToSent_ShouldSucceed()
    {
        // Arrange
        var order = new Order("Test");

        // Act
        order.UpdateStatus(OrderStatus.Sent);

        // Assert
        order.Status.Should().Be(OrderStatus.Sent);
        order.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdateStatus_FromCreatedToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = new Order("Test");

        // Act
        order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateStatus_FromSentToDelivered_ShouldSucceed()
    {
        // Arrange
        var order = new Order("Test");
        order.UpdateStatus(OrderStatus.Sent);

        // Act
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void UpdateStatus_FromSentToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = new Order("Test");
        order.UpdateStatus(OrderStatus.Sent);

        // Act
        order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void UpdateStatus_FromDeliveredToAny_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order("Test");
        order.UpdateStatus(OrderStatus.Sent);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var act = () => order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition from Delivered to Cancelled");
    }

    [Fact]
    public void UpdateStatus_FromCancelledToAny_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order("Test");
        order.UpdateStatus(OrderStatus.Cancelled);

        // Act
        var act = () => order.UpdateStatus(OrderStatus.Sent);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition from Cancelled to Sent");
    }

    [Fact]
    public void UpdateStatus_ToSameStatus_ShouldNotChangeUpdatedAt()
    {
        // Arrange
        var order = new Order("Test");
        var originalUpdatedAt = order.UpdatedAt;

        // Act
        order.UpdateStatus(OrderStatus.Created);

        // Assert
        order.Status.Should().Be(OrderStatus.Created);
        order.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void UpdateStatus_FromCreatedToDelivered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order("Test");

        // Act
        var act = () => order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot transition from Created to Delivered");
    }

    [Fact]
    public void MultipleStatusTransitions_ShouldWorkCorrectly()
    {
        // Arrange
        var order = new Order("Test");

        // Act
        order.UpdateStatus(OrderStatus.Sent);
        order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void TwoDifferentOrders_ShouldHaveDifferentIds()
    {
        // Arrange
        var order1 = new Order("Order 1");
        var order2 = new Order("Order 2");

        // Assert
        order1.Id.Should().NotBe(order2.Id);
    }

    [Fact]
    public void TwoDifferentOrders_ShouldHaveDifferentOrderNumbers()
    {
        // Arrange
        var order1 = new Order("Order 1");
        var order2 = new Order("Order 2");

        // Assert
        order1.OrderNumber.Should().NotBe(order2.OrderNumber);
    }
}