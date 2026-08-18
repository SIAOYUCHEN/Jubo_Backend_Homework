using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IOrderRepository
{
    IQueryable<Order> GetAll();
    IQueryable<Order> GetByPatientId(Guid patientId);
}
