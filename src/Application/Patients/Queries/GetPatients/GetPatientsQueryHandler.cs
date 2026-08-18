using Application.Common.Interfaces;
using Application.Patients.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, List<PatientDto>>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientsQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public Task<List<PatientDto>> Handle(GetPatientsQuery request, CancellationToken cancellationToken) =>
        _patientRepository.GetAll()
            .Select(p => new PatientDto { Id = p.Id, Name = p.Name })
            .ToListAsync(cancellationToken);
}
