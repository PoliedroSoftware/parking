using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Parking.Api.Hubs;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/print")]
public class PrintController(IHubContext<PrintHub> hubContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Print([FromBody] PrintJob job)
    {
        if (!PrintHub.HasPrinters)
            return Ok(new { success = false, message = "No hay agentes de impresion conectados" });

        await hubContext.Clients.All.SendAsync("PrintJob", new
        {
            job.PrinterName,
            job.Content,
            JobId = job.JobId ?? Guid.NewGuid().ToString("N")[..8]
        });

        return Ok(new { success = true, message = "Trabajo enviado a impresora" });
    }

    [HttpGet("status")]
    public ActionResult Status()
    {
        return Ok(new { printersOnline = PrintHub.HasPrinters });
    }
}
