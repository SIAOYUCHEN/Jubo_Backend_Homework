using Application.Patients.Commands.CreatePatient;
using FluentAssertions;

namespace Application.UnitTests.Patients;

public class CreatePatientCommandValidatorTests
{
    private readonly CreatePatientCommandValidator _validator = new();

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        var result = _validator.Validate(new CreatePatientCommand(""));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ValidName_Passes()
    {
        var result = _validator.Validate(new CreatePatientCommand("王小明"));

        result.IsValid.Should().BeTrue();
    }
}
