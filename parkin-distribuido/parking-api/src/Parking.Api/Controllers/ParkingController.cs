using CleanArchitecture.Blazor.Application.Common.Extensions;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Parking.Api.DTOs;
using Parking.Api.Hubs;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/parking"), Authorize]
public class ParkingController(
    IApplicationDbContextFactory dbFactory,
    IHubContext<ParkingHub> hubContext) : ControllerBase
{
    private const string ActiveStatus = "Activo";
    private const string CompletedStatus = "Completado";
    private const int HourlyRate = 3500;

    [HttpPost("entry")]
    public async Task<ActionResult<EntryResponse>> CreateEntry(EntryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Plate))
            return BadRequest(new { error = "Placa requerida" });

        await using var db = await dbFactory.CreateAsync();

        var existing = await db.ParkingRecords
            .Where(p => p.Status == ActiveStatus && p.LicensePlate == request.Plate)
            .AnyAsync();

        if (existing)
            return Conflict(new { error = $"La placa {request.Plate} ya tiene un parqueo activo" });

        var entryTime = DateTime.Now;
        var ticketNo = $"TK-{DateTime.Now:yyMMddHHmmss}";

        var record = new ParkingRecord
        {
            LicensePlate = request.Plate,
            EntryTime = entryTime,
            TicketNumber = ticketNo,
            Status = ActiveStatus,
            Amount = 0,
            Notes = request.VehicleType
        };

        db.ParkingRecords.Add(record);
        await db.SaveChangesAsync(CancellationToken.None);

        var ticketText = GenerateEntryTicket(request.Plate, request.VehicleType, entryTime, ticketNo);

        await hubContext.Clients.Group("parking-operators").SendAsync("ParkingEntry", new
        {
            record.Id,
            Plate = request.Plate,
            VehicleType = request.VehicleType,
            EntryTime = entryTime,
            TicketNumber = ticketNo
        });

        return Ok(new EntryResponse(
            record.Id,
            ticketNo,
            entryTime,
            ticketText,
            GenerateTicketHtml(request.Plate, request.VehicleType, entryTime, ticketNo, "ENTRADA")));
    }

    [HttpPost("exit")]
    public async Task<ActionResult<ExitResponse>> ProcessExit(ExitRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Plate))
            return BadRequest(new { error = "Placa requerida" });

        await using var db = await dbFactory.CreateAsync();

        var record = await db.ParkingRecords
            .Where(p => p.Status == ActiveStatus && p.LicensePlate == request.Plate)
            .OrderByDescending(p => p.EntryTime)
            .FirstOrDefaultAsync();

        if (record is null)
            return NotFound(new { error = $"No hay parqueo activo para la placa {request.Plate}" });

        var exitTime = DateTime.Now;
        var duration = exitTime - record.EntryTime;
        var amount = CalculateAmount(duration);
        var formattedDuration = FormatDuration(duration);
        var customer = string.IsNullOrWhiteSpace(request.CustomerName) ? "Consumidor Final" : request.CustomerName.Trim();

        record.ExitTime = exitTime;
        record.Amount = amount;
        record.Status = CompletedStatus;
        record.CustomerName = customer;
        await db.SaveChangesAsync(CancellationToken.None);

        var ticketText = GenerateExitTicket(record.LicensePlate, record.EntryTime, exitTime, amount, formattedDuration,
            record.TicketNumber ?? "");

        await hubContext.Clients.Group("parking-operators").SendAsync("ParkingExit", new
        {
            Plate = record.LicensePlate,
            Amount = amount,
            Duration = formattedDuration
        });

        return Ok(new ExitResponse(
            amount,
            formattedDuration,
            ticketText,
            GenerateTicketHtml(record.LicensePlate, ParseVehicleType(record.Notes), exitTime, record.TicketNumber ?? "", "SALIDA", amount),
            exitTime,
            record.EntryTime));
    }

    [HttpGet("active")]
    public async Task<ActionResult<List<ActiveParkingDto>>> GetActive()
    {
        await using var db = await dbFactory.CreateAsync();
        var now = DateTime.Now;

        var records = await db.ParkingRecords
            .AsNoTracking()
            .Where(p => p.Status == ActiveStatus)
            .OrderBy(p => p.EntryTime)
            .Select(p => new
            {
                p.LicensePlate,
                p.EntryTime,
                p.TicketNumber,
                p.Notes
            })
            .ToListAsync();

        return Ok(records.Select(r => new ActiveParkingDto(
            r.LicensePlate,
            r.EntryTime,
            ParseVehicleType(r.Notes),
            CalculateAmount(now - r.EntryTime),
            r.TicketNumber ?? ""
        )).ToList());
    }

    [HttpGet("today")]
    public async Task<ActionResult<List<TodayMovementDto>>> GetTodayMovements()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        await using var db = await dbFactory.CreateAsync();

        var records = await db.ParkingRecords
            .AsNoTracking()
            .Where(p => p.EntryTime >= today && p.EntryTime < tomorrow)
            .OrderByDescending(p => p.ExitTime ?? p.EntryTime)
            .ToListAsync();

        return Ok(records.Select(r => new TodayMovementDto(
            r.Id,
            r.LicensePlate,
            r.EntryTime,
            r.ExitTime,
            r.Status,
            (int)r.Amount,
            ParseVehicleType(r.Notes),
            r.CustomerName,
            r.ExitTime.HasValue ? FormatDuration(r.ExitTime.Value - r.EntryTime) : "--",
            r.TicketNumber
        )).ToList());
    }

    private static int CalculateAmount(TimeSpan duration)
    {
        var hours = (int)Math.Ceiling(duration.TotalHours);
        return Math.Max(2000, hours * HourlyRate);
    }

    private static string FormatDuration(TimeSpan d)
    {
        if (d.TotalDays >= 1) return $"{(int)d.TotalDays}d {d.Hours}h {d.Minutes}m";
        if (d.TotalHours >= 1) return $"{(int)d.TotalHours}h {d.Minutes}m";
        return $"{d.Minutes} min";
    }

    private static string ParseVehicleType(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) return "Carro";
        if (System.Enum.TryParse<VehicleTypes>(notes, out var vt))
            return vt.GetDescription();
        return notes.Split('|', 2)[0].Trim();
    }

    private static string GenerateEntryTicket(string plate, string vehicleType, DateTime entryTime, string ticketNo)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[HEADER]");
        sb.AppendLine("[CENTER-SMALL]:POLIEDRO PARKING");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER]:ENTRADA");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Fecha:|{entryTime:dd/MM/yyyy hh:mm tt}");
        sb.AppendLine($"[ROW]:Ticket:|{ticketNo}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-SMALL]:PLACA");
        sb.AppendLine($"[HUGE]:{plate}");
        if (!string.IsNullOrWhiteSpace(vehicleType))
            sb.AppendLine($"[CENTER]:{vehicleType}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-SMALL]:Conserve este ticket");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[CENTER-TINY]:Impreso: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}");
        sb.AppendLine("[CENTER-TINY]:POLIEDRO SOFTWARE");
        return sb.ToString();
    }

    private static string GenerateExitTicket(string plate, DateTime entryTime, DateTime exitTime, int amount,
        string duration, string ticketNo)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[HEADER]");
        sb.AppendLine("[CENTER-SMALL]:POLIEDRO PARKING");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER]:SALIDA");
        sb.AppendLine("[DASHED]");
        sb.AppendLine($"[ROW]:Fecha:|{exitTime:dd/MM/yyyy hh:mm tt}");
        sb.AppendLine($"[ROW]:Ticket:|{ticketNo}");
        sb.AppendLine($"[ROW]:Entrada:|{entryTime:hh:mm tt}");
        sb.AppendLine($"[ROW]:Salida:|{exitTime:hh:mm tt}");
        sb.AppendLine($"[ROW]:Tiempo:|{duration}");
        sb.AppendLine("[DASHED]");
        sb.AppendLine("[CENTER-SMALL]:PLACA");
        sb.AppendLine($"[HUGE]:{plate}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[HUGE]:$ {amount:N0}");
        sb.AppendLine("[DOUBLE]");
        sb.AppendLine($"[CENTER-TINY]:Impreso: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}");
        sb.AppendLine("[CENTER-TINY]:POLIEDRO SOFTWARE");
        return sb.ToString();
    }

    private static string GenerateTicketHtml(string plate, string vehicleType, DateTime time, string ticketNo,
        string type, int amount = 0)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset='utf-8'>");
        sb.AppendLine("<style>@media print{@page{size:80mm 297mm;margin:0}}body{width:72mm;margin:4mm;font-family:'Courier New',monospace;font-size:10px;font-weight:800;color:#000;}");
        sb.AppendLine(".center{text-align:center}.bold{font-weight:900}.line{border-top:1px dashed #000;margin:5px 0}.double{border-top:3px double #000;margin:5px 0}");
        sb.AppendLine(".huge{font-size:22px;text-align:center;font-weight:900}.row{display:flex;justify-content:space-between}</style></head><body>");
        sb.AppendLine("<div class='center bold'>POLIEDRO PARKING</div>");
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='center bold'>{type}</div>");
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='row'><span>Fecha:</span><span>{time:dd/MM/yyyy hh:mm tt}</span></div>");
        sb.AppendLine($"<div class='row'><span>Ticket:</span><span>{ticketNo}</span></div>");
        if (type == "SALIDA" && amount > 0)
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine($"<div class='huge'>{plate}</div>");
            sb.AppendLine("<div class='double'></div>");
            sb.AppendLine($"<div class='huge'>$ {amount:N0}</div>");
            sb.AppendLine("<div class='double'></div>");
        }
        else
        {
            sb.AppendLine("<div class='line'></div>");
            sb.AppendLine("<div class='center'>PLACA</div>");
            sb.AppendLine($"<div class='huge'>{plate}</div>");
            if (!string.IsNullOrWhiteSpace(vehicleType))
                sb.AppendLine($"<div class='center'>{vehicleType}</div>");
        }
        sb.AppendLine("<div class='line'></div>");
        sb.AppendLine($"<div class='center'>Impreso: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}</div>");
        sb.AppendLine("<div class='center'>POLIEDRO SOFTWARE</div>");
        sb.AppendLine("<script>window.onload=function(){setTimeout(function(){window.print();},250);};</script></body></html>");
        return sb.ToString();
    }
}
