using Application.Common.Interfaces;
using Application.Patients.Dtos;
using Domain.Entities;
using MediatR;

namespace Application.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, PatientDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePatientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PatientDto> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = new Patient { Id = Guid.NewGuid(), Name = request.Name };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return new PatientDto { Id = patient.Id, Name = patient.Name };
    }
}
