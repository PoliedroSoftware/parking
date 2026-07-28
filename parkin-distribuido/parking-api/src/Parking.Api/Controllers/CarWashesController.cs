using CleanArchitecture.Blazor.Application.Common.Extensions;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/carwashes"), Authorize]
public class CarWashesController(IApplicationDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        await using var db = await dbFactory.CreateAsync();
        var query = db.CarWashes.AsNoTracking().Where(c => c.StartTime.HasValue);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.StartTime)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new
            {
                c.Id, c.LicensePlate, WashServiceType = c.WashServiceType.GetDescription(),
                Status = c.Status.GetDescription(), c.Price, c.IsPaid,
                PaymentMethod = c.PaymentMethod.GetDescription(),
                c.QueueNumber, c.StartTime, c.EndTime, c.Notes,
                c.VehicleType, c.WeekendSurcharge, c.CommissionTotal
            })
            .ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCarWashRequest request)
    {
        await using var db = await dbFactory.CreateAsync();
        var entity = new CarWash
        {
            LicensePlate = request.LicensePlate,
            VehicleType = request.VehicleType,
            WashServiceType = request.WashServiceType,
            Price = request.Price,
            StartTime = DateTime.Now,
            Status = CarWashStatus.Pending,
            Notes = request.Notes,
            QueueNumber = request.QueueNumber
        };
        db.CarWashes.Add(entity);
        await db.SaveChangesAsync(CancellationToken.None);
        return Ok(new { id = entity.Id });
    }

    [HttpPut("{id}/pay")]
    public async Task<ActionResult> Pay(int id, [FromBody] PayRequest request)
    {
        await using var db = await dbFactory.CreateAsync();
        var entity = await db.CarWashes.FindAsync(id);
        if (entity is null) return NotFound();
        entity.IsPaid = true;
        entity.PaymentMethod = request.PaymentMethod;
        await db.SaveChangesAsync(CancellationToken.None);
        return Ok(new { success = true });
    }

    [HttpPost("{id}/print")]
    public async Task<ActionResult> PrintTicket(int id)
    {
        await using var db = await dbFactory.CreateAsync();
        var entity = await db.CarWashes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (entity is null) return NotFound();
        var text = GenerateWashTicketText(entity);
        var html = GenerateWashTicketHtml(entity);
        return Ok(new { ticketText = text, ticketHtml = html });
    }

    private static string GenerateWashTicketText(CarWash w)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[HEADER]");
        sb.AppendLine("[CENTER]:LAVADO");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Fecha:|{DateTime.Now:dd/MM/yyyy hh:mm tt}");
        if (w.QueueNumber > 0) sb.AppendLine($"[CENTER]:TURNO #{w.QueueNumber}");
        sb.AppendLine($"[CENTER]:{w.WashServiceType.GetDescription()}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-SMALL]:PLACA");
        sb.AppendLine($"[HUGE]:{w.LicensePlate}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {(int)w.Price:N0}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[CENTER]:{w.PaymentMethod.GetDescription()}");
        return sb.ToString();
    }

    private static string GenerateWashTicketHtml(CarWash w)
    {
        return $"<!DOCTYPE html><html><head><meta charset='utf-8'><style>@media print{{@page{{size:80mm 297mm;margin:0}}}}body{{width:72mm;margin:4mm;font-family:'Courier New',monospace;font-size:10px;font-weight:800}}</style></head><body><div style='text-align:center;font-weight:900'>LAVADO</div><div style='border-top:1px dashed #000;margin:5px 0'></div><div style='text-align:center;font-size:22px;font-weight:900'>{w.LicensePlate}</div><div style='text-align:center'>{w.WashServiceType.GetDescription()}</div><div style='border-top:3px double #000;margin:5px 0'></div><div style='text-align:center;font-size:22px;font-weight:900'>$ {(int)w.Price:N0}</div><div style='border-top:3px double #000;margin:5px 0'></div><script>window.onload=function(){{setTimeout(function(){{window.print();}},250);}};</script></body></html>";
    }

    public record CreateCarWashRequest(
        string LicensePlate, VehicleTypes VehicleType, WashServiceType WashServiceType,
        decimal Price, string? Notes = null, int QueueNumber = 0);
    public record PayRequest(PaymentMethods PaymentMethod);
}
