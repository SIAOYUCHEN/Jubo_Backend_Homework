using Application.Patients.Commands.CreatePatient;
using Application.Patients.Commands.DeletePatient;
using Application.Patients.Commands.UpdatePatient;
using Application.Patients.Dtos;
using Application.Patients.Queries.GetPatientById;
using Application.Patients.Queries.GetPatients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Contracts.Patients;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly ISender _sender;

    public PatientsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<PatientDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPatientsQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPatientByIdQuery(id), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create(PatientRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreatePatientCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PatientDto>> Update(Guid id, PatientRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new UpdatePatientCommand(id, request.Name), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeletePatientCommand(id), cancellationToken);
        return NoContent();
    }
}
