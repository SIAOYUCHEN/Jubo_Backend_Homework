using Application.Common.Exceptions;
using Application.Orders.Commands.UpdateOrder;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;

namespace Application.UnitTests.Orders;

public class UpdateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingOrder_UpdatesMessageAndTimestamp()
    {
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            PatientId = SeedData.PatientIds[0],
            Message = "舊訊息",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateOrderCommandHandler(context);
        var result = await handler.Handle(new UpdateOrderCommand(order.Id, "新訊息"), CancellationToken.None);

        result.Message.Should().Be("新訊息");
        result.UpdatedAt.Should().BeAfter(result.CreatedAt);
    }

    [Fact]
    public async Task Handle_UnknownOrder_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdateOrderCommandHandler(context);

        var act = () => handler.Handle(new UpdateOrderCommand(Guid.NewGuid(), "x"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
