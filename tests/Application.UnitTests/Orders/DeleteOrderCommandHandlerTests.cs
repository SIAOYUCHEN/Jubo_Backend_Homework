using Application.Common.Exceptions;
using Application.Orders.Commands.DeleteOrder;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Orders;

public class DeleteOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingOrder_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            PatientId = SeedData.PatientIds[0],
            Message = "test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteOrderCommandHandler(context);
        await handler.Handle(new DeleteOrderCommand(order.Id), CancellationToken.None);

        (await context.Orders.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeleteOrderCommandHandler(context);

        var act = () => handler.Handle(new DeleteOrderCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
