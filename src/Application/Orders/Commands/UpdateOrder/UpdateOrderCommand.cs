using Application.Orders.Dtos;
using MediatR;

namespace Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid Id, string Message) : IRequest<OrderDto>;
