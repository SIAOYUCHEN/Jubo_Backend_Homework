using Application.Common.Interfaces;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Moq;

namespace Application.UnitTests.Repositories;

public class PatientRepositoryTests
{
    [Fact]
    public void GetAll_ReturnsQueryable_WithoutMaterializingIt()
    {
        var patients = new[] { new Patient { Id = Guid.NewGuid(), Name = "王小明" } };
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(c => c.Patients).Returns(MockDbSetFactory.CreateNonEnumerable(patients));

        var repository = new PatientRepository(context.Object);

        var act = () => repository.GetAll();

        act.Should().NotThrow("the repository must return the query lazily, not enumerate it");
    }
}
