using Application.Common.Exceptions;
using Application.Patients.Queries.GetPatientById;
using Application.UnitTests.TestUtils;
using FluentAssertions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;

namespace Application.UnitTests.Patients;

public class GetPatientByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingId_ReturnsPatient()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetPatientByIdQueryHandler(new PatientRepository(context));

        var result = await handler.Handle(new GetPatientByIdQuery(SeedData.PatientIds[0]), CancellationToken.None);

        result.Id.Should().Be(SeedData.PatientIds[0]);
    }

    [Fact]
    public async Task Handle_UnknownId_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new GetPatientByIdQueryHandler(new PatientRepository(context));

        var act = () => handler.Handle(new GetPatientByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
