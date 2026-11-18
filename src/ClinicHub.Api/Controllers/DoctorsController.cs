using ClinicHub.Application.Doctors;
using ClinicHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Api.Controllers;

[ApiController]
[Route("api/doctors")]
public class DoctorsController(DoctorService doctors) : ControllerBase
{
    [HttpGet]
    public Task<List<DoctorDto>> List(CancellationToken ct) => doctors.ListAsync(ct);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<DoctorDto>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateDoctorRequest request, CancellationToken ct)
    {
        var created = await doctors.CreateAsync(request, ct);
        return Created($"/api/doctors/{created.Id}", created);
    }
}
