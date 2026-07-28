using CleanArchitecture.Blazor.Application.Common.Extensions;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/members"), Authorize]
public class MembersController(IApplicationDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        await using var db = await dbFactory.CreateAsync();
        var query = db.Members.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search) || (m.LicensePlate ?? "").Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new
            {
                m.Id, m.Name, m.LicensePlate, m.CardId, m.PhoneNumber,
                m.StartDate, m.ExpiryDate, m.IsActive, m.Notes
            })
            .ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}/rentals")]
    public async Task<ActionResult> GetRentals(int id)
    {
        await using var db = await dbFactory.CreateAsync();
        var rentals = await db.MemberRentals.AsNoTracking()
            .Where(r => r.MemberId == id)
            .OrderByDescending(r => r.PaymentTime)
            .Select(r => new
            {
                r.Id, r.StartDate, r.ExpiryDate, r.RentalFee, r.Deposit,
                r.AmountDue, r.AmountPaid, r.PaymentTime,
                PaymentMethod = r.PaymentMethodId.GetDescription(),
                r.LicensePlate, r.CardId, r.Notes,
                PaidMonth = r.StartDate.HasValue
                    ? System.Globalization.CultureInfo.GetCultureInfo("es-CO").TextInfo.ToTitleCase(
                        r.StartDate.Value.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-CO")))
                    : ""
            })
            .ToListAsync();
        return Ok(rentals);
    }

    [HttpPost("{id}/pay")]
    public async Task<ActionResult> Pay(int id, [FromBody] MemberPayRequest request)
    {
        if (request.TotalAmount <= 0) return BadRequest(new { error = "Monto invalido" });

        await using var db = await dbFactory.CreateAsync();
        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (member is null) return NotFound(new { error = "Miembro no encontrado" });

        var rental = new MemberRental
        {
            MemberId = id,
            LicensePlate = member.LicensePlate,
            CardId = member.CardId,
            StartDate = DateTime.Today,
            ExpiryDate = DateTime.Today.AddMonths(1),
            RentalFee = request.TotalAmount,
            AmountDue = request.TotalAmount,
            AmountPaid = request.TotalAmount,
            PaymentTime = DateTime.Now,
            PaymentMethodId = request.PaymentMethod
        };

        db.MemberRentals.Add(rental);
        await db.SaveChangesAsync(CancellationToken.None);
        return Ok(new { rentalId = rental.Id, success = true });
    }

    [HttpPost("{id}/print")]
    public async Task<ActionResult> PrintReceipt(int id, [FromQuery] int? rentalId = null)
    {
        await using var db = await dbFactory.CreateAsync();
        MemberRental? rental;
        if (rentalId.HasValue)
            rental = await db.MemberRentals.AsNoTracking().FirstOrDefaultAsync(r => r.Id == rentalId.Value);
        else
            rental = await db.MemberRentals.AsNoTracking()
                .Where(r => r.MemberId == id)
                .OrderByDescending(r => r.PaymentTime)
                .FirstOrDefaultAsync();

        if (rental is null) return NotFound(new { error = "No hay pagos registrados" });

        var member = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        var vehicles = await db.MemberVehicles.AsNoTracking()
            .Where(v => v.MemberId == id && v.Vehicle != null)
            .Select(v => new VehicleInfo(v.Vehicle!.Name, v.Vehicle.VehicleTypeId))
            .ToListAsync();

        var text = GenerateMonthlyTicketText(member?.Name ?? "", member?.LicensePlate ?? "",
            vehicles, rental);
        var html = GenerateMonthlyTicketHtml(member?.Name ?? "", member?.LicensePlate ?? "",
            vehicles, rental);

        return Ok(new { ticketText = text, ticketHtml = html });
    }

    private static string GenerateMonthlyTicketText(string name, string plate,
        List<VehicleInfo> vehicles, MemberRental r)
    {
        var method = r.PaymentMethodId.GetDescription();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[HEADER]");
        sb.AppendLine("[CENTER]:MENSUALIDAD");
        sb.AppendLine("[DASHED]");
        if (!string.IsNullOrWhiteSpace(name))
            sb.AppendLine($"[ROW]:Cliente:|{name}");
        sb.AppendLine($"[ROW]:Mes pagado:|{r.StartDate:MMMM yyyy}");
        foreach (var v in vehicles)
            sb.AppendLine($"[ROW]:{v.Name}|{v.VehicleTypeId.GetDescription()}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Valor alquiler:|$ {(int)r.AmountPaid:N0}");
        sb.AppendLine($"[ROW]:Metodo de pago:|{method}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-SMALL]:Impreso: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}");
        sb.AppendLine("[CENTER-TINY]:POLIEDRO SOFTWARE");
        return sb.ToString();
    }

    private static string GenerateMonthlyTicketHtml(string name, string plate,
        List<VehicleInfo> vehicles, MemberRental r)
    {
        var method = r.PaymentMethodId.GetDescription();
        var sb = new System.Text.StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='utf-8'><style>@media print{@page{size:80mm 297mm;margin:0}}body{width:72mm;margin:4mm;font-family:'Courier New',monospace;font-size:10px;font-weight:800}</style></head><body>");
        sb.Append("<div style='text-align:center;font-weight:900'>MENSUALIDAD</div><div style='border-top:1px dashed #000;margin:5px 0'></div>");
        if (!string.IsNullOrWhiteSpace(name))
            sb.Append($"<div style='display:flex;justify-content:space-between'><span>Cliente:</span><span>{name}</span></div>");
        sb.Append($"<div style='display:flex;justify-content:space-between'><span>Mes pagado:</span><span>{r.StartDate:MMMM yyyy}</span></div>");
        foreach (var v in vehicles)
            sb.Append($"<div style='display:flex;justify-content:space-between'><span>{v.Name}</span><span>{v.VehicleTypeId.GetDescription()}</span></div>");
        sb.Append("<div style='border-top:1px dashed #000;margin:5px 0'></div>");
        sb.Append($"<div style='display:flex;justify-content:space-between;font-weight:900'><span>Valor alquiler:</span><span>$ {(int)r.AmountPaid:N0}</span></div>");
        sb.Append($"<div style='text-align:center'>Metodo de pago: {method}</div>");
        sb.Append("<script>window.onload=function(){setTimeout(function(){window.print();},250);};</script></body></html>");
        return sb.ToString();
    }

    public record MemberPayRequest(int TotalAmount, PaymentMethods PaymentMethod);
    public record MemberPrintRequest(int MemberId, int? RentalId, PaymentMethods? PaymentMethod);
    public record VehicleInfo(string Name, VehicleTypes VehicleTypeId);
}
