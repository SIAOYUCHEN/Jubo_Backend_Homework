using Application.Patients.Dtos;
using MediatR;

namespace Application.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto>;
