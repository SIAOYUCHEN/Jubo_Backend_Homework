using Application.Patients.Dtos;
using MediatR;

namespace Application.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(Guid Id, string Name) : IRequest<PatientDto>;
