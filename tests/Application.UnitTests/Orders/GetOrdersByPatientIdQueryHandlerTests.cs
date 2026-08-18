using Application.Common.Interfaces;
using Application.Orders.Queries.GetOrdersByPatientId;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Moq;

namespace Application.UnitTests.Orders;

public class GetOrdersByPatientIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyOrdersForThatPatient()
    {
        using var context = TestDbContextFactory.Create();
        var patientId = SeedData.PatientIds[0];
        var otherPatientId = SeedData.PatientIds[1];
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "A", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "B", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), PatientId = otherPatientId, Message = "C", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var handler = new GetOrdersByPatientIdQueryHandler(new OrderRepository(context));

        var result = await handler.Handle(new GetOrdersByPatientIdQuery(patientId), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.PatientId == patientId);
    }

    [Fact]
    public async Task Handle_QueriesRepositoryExactlyOnce_NoNPlusOne()
    {
        using var context = TestDbContextFactory.Create();
        var patientId = SeedData.PatientIds[0];
        context.Orders.AddRange(
            new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "A", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "B", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "C", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync(CancellationToken.None);

        var callCount = 0;
        var innerRepository = new OrderRepository(context);
        var spyRepository = new Mock<IOrderRepository>();
        spyRepository
            .Setup(r => r.GetByPatientId(patientId))
            .Returns(() =>
            {
                callCount++;
                return innerRepository.GetByPatientId(patientId);
            });

        var handler = new GetOrdersByPatientIdQueryHandler(spyRepository.Object);

        var result = await handler.Handle(new GetOrdersByPatientIdQuery(patientId), CancellationToken.None);

        result.Should().HaveCount(3);
        callCount.Should().Be(1, "the handler must fetch all orders for the patient in a single query, not once per order");
    }
}
