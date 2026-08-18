using Application.Common.Behaviours;
using Application.Common.Exceptions;
using Application.Patients.Commands.CreatePatient;
using FluentAssertions;
using MediatR;

namespace Application.UnitTests.Common;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var behavior = new ValidationBehavior<CreatePatientCommand, object>(
            new[] { new CreatePatientCommandValidator() });

        var act = () => behavior.Handle(
            new CreatePatientCommand(""),
            () => Task.FromResult(new object()),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var behavior = new ValidationBehavior<CreatePatientCommand, object>(
            new[] { new CreatePatientCommandValidator() });
        var expected = new object();

        var result = await behavior.Handle(
            new CreatePatientCommand("王小明"),
            () => Task.FromResult(expected),
            CancellationToken.None);

        result.Should().BeSameAs(expected);
    }
}
