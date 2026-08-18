using Application.Common.Interfaces;
using Application.Orders.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries.GetOrdersByPatientId;

public class GetOrdersByPatientIdQueryHandler : IRequestHandler<GetOrdersByPatientIdQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByPatientIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public Task<List<OrderDto>> Handle(GetOrdersByPatientIdQuery request, CancellationToken cancellationToken) =>
        _orderRepository.GetByPatientId(request.PatientId)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                PatientId = o.PatientId,
                Message = o.Message,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt,
            })
            .ToListAsync(cancellationToken);
}
