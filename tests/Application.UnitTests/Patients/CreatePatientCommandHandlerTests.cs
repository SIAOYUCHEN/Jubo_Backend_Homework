using Application.Patients.Commands.CreatePatient;
using Application.UnitTests.TestUtils;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Patients;

public class CreatePatientCommandHandlerTests
{
    [Fact]
    public async Task Handle_AddsPatientAndReturnsDto()
    {
        using var context = TestDbContextFactory.Create();
        var handler = new CreatePatientCommandHandler(context);

        var result = await handler.Handle(new CreatePatientCommand("新病患"), CancellationToken.None);

        result.Name.Should().Be("新病患");
        (await context.Patients.CountAsync()).Should().Be(6);
    }
}
