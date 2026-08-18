using Application.Patients.Dtos;
using MediatR;

namespace Application.Patients.Commands.CreatePatient;

public record CreatePatientCommand(string Name) : IRequest<PatientDto>;
