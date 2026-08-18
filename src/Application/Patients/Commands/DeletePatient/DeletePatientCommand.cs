using MediatR;

namespace Application.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest;
