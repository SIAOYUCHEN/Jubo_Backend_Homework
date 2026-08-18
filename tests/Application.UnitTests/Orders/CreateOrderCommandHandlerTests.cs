using Application.Common.Exceptions;
using Application.Orders.Commands.CreateOrder;
using Application.UnitTests.TestUtils;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Orders;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingPatient_AddsOrder()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateOrderCommandHandler(context);

        var result = await handler.Handle(
            new CreateOrderCommand(SeedData.PatientIds[0], "服用普拿疼"), CancellationToken.None);

        result.Message.Should().Be("服用普拿疼");
        result.CreatedAt.Should().Be(result.UpdatedAt);
        (await context.Orders.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Handle_UnknownPatient_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreateOrderCommandHandler(context);

        var act = () => handler.Handle(new CreateOrderCommand(Guid.NewGuid(), "x"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
