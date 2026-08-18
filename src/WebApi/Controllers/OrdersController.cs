using Application.Orders.Commands.CreateOrder;
using Application.Orders.Commands.DeleteOrder;
using Application.Orders.Commands.UpdateOrder;
using Application.Orders.Dtos;
using Application.Orders.Queries.GetOrdersByPatientId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.Orders;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("api/patients/{patientId:guid}/orders")]
    public async Task<ActionResult<List<OrderDto>>> GetByPatientId(Guid patientId, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetOrdersByPatientIdQuery(patientId), cancellationToken));

    [HttpPost("api/patients/{patientId:guid}/orders")]
    public async Task<ActionResult<OrderDto>> Create(Guid patientId, OrderRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateOrderCommand(patientId, request.Message), cancellationToken);
        return CreatedAtAction(nameof(GetByPatientId), new { patientId }, result);
    }

    [HttpPut("api/orders/{id:guid}")]
    public async Task<ActionResult<OrderDto>> Update(Guid id, OrderRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new UpdateOrderCommand(id, request.Message), cancellationToken));

    [HttpDelete("api/orders/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteOrderCommand(id), cancellationToken);
        return NoContent();
    }
}
