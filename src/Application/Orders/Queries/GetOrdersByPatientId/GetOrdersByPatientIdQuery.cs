using Application.Orders.Dtos;
using MediatR;

namespace Application.Orders.Queries.GetOrdersByPatientId;

public record GetOrdersByPatientIdQuery(Guid PatientId) : IRequest<List<OrderDto>>;
