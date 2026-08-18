using Application.Common.Interfaces;
using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IApplicationDbContext _context;

    public OrderRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public IQueryable<Order> GetAll() => _context.Orders.AsQueryable();

    public IQueryable<Order> GetByPatientId(Guid patientId) =>
        _context.Orders.Where(o => o.PatientId == patientId);
}
