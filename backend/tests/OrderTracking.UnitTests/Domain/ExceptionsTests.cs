using FluentAssertions;
using OrderTracking.Domain.Exceptions;

namespace OrderTracking.UnitTests.Domain;

public class OrderNotFoundExceptionTests
{
    [Fact]
    public void OrderNotFoundException_ShouldHaveCorrectStatusCode()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var exception = new OrderNotFoundException(orderId);

        // Assert
        exception.StatusCode.Should().Be(404);
        exception.ErrorCode.Should().Be("ORDER_NOT_FOUND");
        exception.Message.Should().Contain(orderId.ToString());
        exception.OrderId.Should().Be(orderId);
    }
}

public class ValidationExceptionTests
{
    [Fact]
    public void ValidationException_WithMessage_ShouldHaveCorrectStatusCode()
    {
        // Arrange
        var exception = new ValidationException("Validation failed");

        // Assert
        exception.StatusCode.Should().Be(400);
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
        exception.Message.Should().Be("Validation failed");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationException_WithErrors_ShouldContainErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Description", new[] { "Description is required" } }
        };

        // Act
        var exception = new ValidationException("Validation failed", errors);

        // Assert
        exception.Errors.Should().ContainKey("Description");
        exception.Errors["Description"].Should().Contain("Description is required");
    }
}

public class DomainExceptionTests
{
    private class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
        public override int StatusCode => 500;
        public override string ErrorCode => "TEST_ERROR";
    }

    [Fact]
    public void DomainException_ShouldBeAbstract()
    {
        // Assert
        typeof(DomainException).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void ConcreteDomainException_ShouldHaveCorrectProperties()
    {
        // Arrange
        var exception = new TestDomainException("Test error");

        // Assert
        exception.StatusCode.Should().Be(500);
        exception.ErrorCode.Should().Be("TEST_ERROR");
        exception.Message.Should().Be("Test error");
    }
}