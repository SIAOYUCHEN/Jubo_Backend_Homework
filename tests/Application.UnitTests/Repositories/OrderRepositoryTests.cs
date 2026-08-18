using Application.Common.Interfaces;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Moq;

namespace Application.UnitTests.Repositories;

public class OrderRepositoryTests
{
    [Fact]
    public void GetAll_ReturnsQueryable_WithoutMaterializingIt()
    {
        var orders = new[] { new Order { Id = Guid.NewGuid(), PatientId = Guid.NewGuid(), Message = "test" } };
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(c => c.Orders).Returns(MockDbSetFactory.CreateNonEnumerable(orders));

        var repository = new OrderRepository(context.Object);

        var act = () => repository.GetAll();

        act.Should().NotThrow("the repository must return the query lazily, not enumerate it");
    }

    [Fact]
    public void GetByPatientId_ReturnsQueryable_WithoutMaterializingIt()
    {
        var patientId = Guid.NewGuid();
        var orders = new[] { new Order { Id = Guid.NewGuid(), PatientId = patientId, Message = "test" } };
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(c => c.Orders).Returns(MockDbSetFactory.CreateNonEnumerable(orders));

        var repository = new OrderRepository(context.Object);

        var act = () => repository.GetByPatientId(patientId);

        act.Should().NotThrow("filtering by patient id must not force enumeration inside the repository");
    }
}
