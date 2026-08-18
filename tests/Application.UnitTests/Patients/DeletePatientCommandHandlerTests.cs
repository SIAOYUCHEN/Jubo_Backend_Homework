using Application.Common.Exceptions;
using Application.Patients.Commands.DeletePatient;
using Application.UnitTests.TestUtils;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Patients;

public class DeletePatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingPatient_RemovesIt()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeletePatientCommandHandler(context);

        await handler.Handle(new DeletePatientCommand(SeedData.PatientIds[0]), CancellationToken.None);

        (await context.Patients.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Handle_UnknownPatient_ThrowsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new DeletePatientCommandHandler(context);

        var act = () => handler.Handle(new DeletePatientCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
