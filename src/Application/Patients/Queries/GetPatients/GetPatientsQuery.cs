using Application.Patients.Dtos;
using MediatR;

namespace Application.Patients.Queries.GetPatients;

public record GetPatientsQuery : IRequest<List<PatientDto>>;
