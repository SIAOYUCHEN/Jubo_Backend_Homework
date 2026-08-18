using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IUserRepository
{
    IQueryable<User> GetAll();
}
