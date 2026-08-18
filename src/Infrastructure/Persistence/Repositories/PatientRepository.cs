using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly IApplicationDbContext _context;

    public PatientRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Patient> GetAll() => _context.Patients.AsQueryable();
}
