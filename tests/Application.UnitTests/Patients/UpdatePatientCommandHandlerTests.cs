using Application.Common.Exceptions;
using Application.Patients.Commands.UpdatePatient;
using Application.UnitTests.TestUtils;
using FluentAssertions;
using Infrastructure.Persistence;

namespace Application.UnitTests.Patients;

public class UpdatePatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingPatient_UpdatesName()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdatePatientCommandHandler(context);

        var result = await handler.Handle(
            new UpdatePatientCommand(SeedData.PatientIds[0], "改名後"), CancellationToken.None);

        result.Name.Should().Be("改名後");
    }

    [Fact]
    public async Task Handle_UnknownPatient_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new UpdatePatientCommandHandler(context);

        var act = () => handler.Handle(new UpdatePatientCommand(Guid.NewGuid(), "x"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
