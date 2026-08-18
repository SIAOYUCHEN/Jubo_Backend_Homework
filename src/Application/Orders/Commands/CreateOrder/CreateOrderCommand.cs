using Application.Orders.Dtos;
using MediatR;

namespace Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(Guid PatientId, string Message) : IRequest<OrderDto>;
