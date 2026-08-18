using Application.Orders.Commands.CreateOrder;
using Application.Orders.Commands.UpdateOrder;
using FluentAssertions;

namespace Application.UnitTests.Orders;

public class OrderValidatorsTests
{
    [Fact]
    public void CreateOrderValidator_EmptyMessage_Fails()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand(Guid.NewGuid(), ""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateOrderValidator_ValidCommand_Passes()
    {
        var result = new CreateOrderCommandValidator().Validate(new CreateOrderCommand(Guid.NewGuid(), "message"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateOrderValidator_EmptyMessage_Fails()
    {
        var result = new UpdateOrderCommandValidator().Validate(new UpdateOrderCommand(Guid.NewGuid(), ""));

        result.IsValid.Should().BeFalse();
    }
}
