using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Patients.Dtos;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, PatientDto>
{
    private readonly IPatientRepository _patientRepository;

    public GetPatientByIdQueryHandler(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<PatientDto> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetAll()
            .Where(p => p.Id == request.Id)
            .Select(p => new PatientDto { Id = p.Id, Name = p.Name })
            .FirstOrDefaultAsync(cancellationToken);

        return patient ?? throw new NotFoundException(nameof(Patient), request.Id);
    }
}
