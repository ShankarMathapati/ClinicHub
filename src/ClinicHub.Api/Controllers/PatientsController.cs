using ClinicHub.Application.Common;
using ClinicHub.Application.Patients;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController(PatientService patients) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<PatientDto>> Search([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => patients.SearchAsync(search, page, pageSize, ct);

    [HttpGet("{id:guid}")]
    public Task<PatientDto> Get(Guid id, CancellationToken ct) => patients.GetAsync(id, ct);

    [HttpPost]
    [ProducesResponseType<PatientDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(SavePatientRequest request, CancellationToken ct)
    {
        var created = await patients.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public Task<PatientDto> Update(Guid id, SavePatientRequest request, CancellationToken ct)
        => patients.UpdateAsync(id, request, ct);
}
