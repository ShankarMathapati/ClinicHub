using ClinicHub.Application.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(AppointmentService appointments, TimeProvider clock) : ControllerBase
{
    /// <summary>Appointments overlapping the range (defaults to the next 7 days).</summary>
    [HttpGet]
    public Task<List<AppointmentDto>> List([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc, [FromQuery] Guid? doctorId, CancellationToken ct)
    {
        var from = fromUtc?.ToUniversalTime() ?? clock.GetUtcNow().UtcDateTime.Date;
        var to = toUtc?.ToUniversalTime() ?? from.AddDays(7);
        return appointments.ListAsync(from, to, doctorId, ct);
    }

    [HttpPost]
    [ProducesResponseType<AppointmentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Book(BookAppointmentRequest request, CancellationToken ct)
    {
        var booked = await appointments.BookAsync(request, ct);
        return Created($"/api/appointments/{booked.Id}", booked);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        await appointments.CancelAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await appointments.CompleteAsync(id, ct);
        return NoContent();
    }
}
