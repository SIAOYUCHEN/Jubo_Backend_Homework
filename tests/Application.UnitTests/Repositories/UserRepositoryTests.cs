using Application.Common.Interfaces;
using Application.UnitTests.TestUtils;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;
using Moq;

namespace Application.UnitTests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public void GetAll_ReturnsQueryable_WithoutMaterializingIt()
    {
        var users = new[] { new User { Id = Guid.NewGuid(), Username = "demo", PasswordHash = "hash" } };
        var context = new Mock<IApplicationDbContext>();
        context.SetupGet(c => c.Users).Returns(MockDbSetFactory.CreateNonEnumerable(users));

        var repository = new UserRepository(context.Object);

        var act = () => repository.GetAll();

        act.Should().NotThrow("the repository must return the query lazily, not enumerate it");
    }
}
