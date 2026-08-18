using Application.UnitTests.TestUtils;
using Application.Patients.Queries.GetPatients;
using FluentAssertions;
using Infrastructure.Persistence.Repositories;

namespace Application.UnitTests.Patients;

public class GetPatientsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllSeededPatients()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetPatientsQueryHandler(new PatientRepository(context));

        var result = await handler.Handle(new GetPatientsQuery(), CancellationToken.None);

        result.Should().HaveCount(5);
        result.Should().OnlyContain(p => p.Id != Guid.Empty && !string.IsNullOrWhiteSpace(p.Name));
    }
}
