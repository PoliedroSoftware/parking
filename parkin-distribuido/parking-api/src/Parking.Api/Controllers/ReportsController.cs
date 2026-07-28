using CleanArchitecture.Blazor.Application.Common.Extensions;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Parking.Api.Controllers;

[ApiController, Route("api/v1/reports"), Authorize]
public class ReportsController(IApplicationDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet("arqueo")]
    public async Task<ActionResult> Arqueo()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        await using var db = await dbFactory.CreateAsync();

        var parkings = await db.ParkingRecords.AsNoTracking()
            .Where(p => p.EntryTime >= today && p.EntryTime < tomorrow)
            .OrderBy(p => p.EntryTime)
            .ToListAsync();

        var washes = await db.CarWashes.AsNoTracking()
            .Where(c => c.StartTime.HasValue && c.StartTime.Value >= today && c.StartTime.Value < tomorrow && c.IsPaid)
            .OrderBy(c => c.StartTime)
            .ToListAsync();

        var rentals = await db.MemberRentals.AsNoTracking()
            .Where(r => r.PaymentTime >= today && r.PaymentTime < tomorrow)
            .OrderBy(r => r.PaymentTime)
            .ToListAsync();

        var totalEntradas = parkings.Count;
        var totalSalidas = parkings.Count(p => p.Status == "Completado");
        var totalParqueo = (int)parkings.Where(p => p.Status == "Completado").Sum(p => p.Amount);
        var totalLavados = washes.Count;
        var totalLavado = (int)washes.Sum(w => w.Price);
        var totalMensualidades = rentals.Count;
        var totalMensualidad = (int)rentals.Sum(r => r.AmountPaid);
        var totalIngresos = totalParqueo + totalLavado + totalMensualidad;

        var movements = new List<object>();
        foreach (var p in parkings.Where(p => p.Status == "Completado"))
            movements.Add(new { Time = p.ExitTime ?? p.EntryTime, Type = "PARQUEO", Placa = p.LicensePlate,
                Cliente = string.IsNullOrEmpty(p.CustomerName) ? "Consumidor Final" : p.CustomerName, Valor = (int)p.Amount });
        foreach (var w in washes)
            movements.Add(new { Time = w.StartTime, Type = "LAVADO", Placa = w.LicensePlate, Cliente = "", Valor = (int)w.Price });
        foreach (var r in rentals)
            movements.Add(new { Time = r.PaymentTime, Type = "MENSUAL", Placa = r.LicensePlate, Cliente = "", Valor = (int)r.AmountPaid });

        return Ok(new
        {
            totalEntradas, totalSalidas, totalLavados, totalMensualidades,
            totalParqueo, totalLavado, totalMensualidad, totalIngresos,
            movements = movements.OrderByDescending(m => ((DateTime)((dynamic)m).Time)).ToList()
        });
    }

    [HttpGet("estatus")]
    public async Task<ActionResult> EstatusTurno()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        await using var db = await dbFactory.CreateAsync();

        var parkings = await db.ParkingRecords.AsNoTracking()
            .Where(p => p.Status == "Activo")
            .OrderBy(p => p.EntryTime)
            .Select(p => new { p.LicensePlate, p.EntryTime, p.Notes })
            .ToListAsync();

        var washes = await db.CarWashes.AsNoTracking()
            .Where(c => c.StartTime.HasValue && c.StartTime.Value >= today && c.StartTime.Value < tomorrow)
            .OrderBy(c => c.StartTime)
            .Select(c => new { c.LicensePlate, Servicio = c.WashServiceType.GetDescription(),
                Estado = c.Status.GetDescription(), c.Price, c.IsPaid })
            .ToListAsync();

        var pagosDia = (int)(await db.ParkingRecords
            .Where(p => p.Status == "Completado" && p.ExitTime.HasValue &&
                        p.ExitTime.Value >= today && p.ExitTime.Value < tomorrow)
            .SumAsync(p => (decimal?)p.Amount) ?? 0)
            + (int)(await db.CarWashes.Where(c => c.IsPaid && c.StartTime.HasValue &&
                        c.StartTime.Value >= today && c.StartTime.Value < tomorrow).SumAsync(c => (decimal?)c.Price) ?? 0)
            + (int)(await db.MemberRentals.Where(r => r.PaymentTime >= today && r.PaymentTime < tomorrow)
                        .SumAsync(r => (decimal?)r.AmountPaid) ?? 0);

        return Ok(new { parkings, washes, pagosDia, totalParkings = parkings.Count,
            totalWashes = washes.Count, lavadosEntregados = washes.Count(w => w.Estado == "Entregado" || w.Estado == "Completado"),
            lavadosPendientes = washes.Count(w => w.Estado != "Entregado" && w.Estado != "Completado" && w.Estado != "Cancelado") });
    }

    [HttpGet("mensual")]
    public async Task<ActionResult> Mensual([FromQuery] int? month, [FromQuery] int? year)
    {
        var m = month ?? DateTime.Today.Month;
        var y = year ?? DateTime.Today.Year;
        var firstDay = new DateTime(y, m, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        await using var db = await dbFactory.CreateAsync();

        var parkingByDay = (await db.ParkingRecords.AsNoTracking()
            .Where(p => p.Status == "Completado" && p.ExitTime.HasValue &&
                        p.ExitTime.Value >= firstDay && p.ExitTime.Value <= lastDay)
            .Select(p => new { p.ExitTime, p.Amount }).ToListAsync())
            .GroupBy(p => p.ExitTime!.Value.Date)
            .ToDictionary(g => g.Key, g => (int)g.Sum(p => p.Amount));

        var washByDay = (await db.CarWashes.AsNoTracking()
            .Where(c => c.StartTime.HasValue && c.IsPaid &&
                        c.StartTime.Value >= firstDay && c.StartTime.Value <= lastDay)
            .Select(c => new { c.StartTime, Price = (int?)c.Price }).ToListAsync())
            .GroupBy(c => c.StartTime!.Value.Date)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Price ?? 0));

        var monthlyByDay = (await db.MemberRentals.AsNoTracking()
            .Where(r => r.PaymentTime >= firstDay && r.PaymentTime <= lastDay)
            .Select(r => new { r.PaymentTime, r.AmountPaid }).ToListAsync())
            .GroupBy(r => r.PaymentTime.Date)
            .ToDictionary(g => g.Key, g => (int)g.Sum(r => r.AmountPaid));

        var expenses = await db.Expenses.AsNoTracking()
            .Where(e => e.Date >= firstDay && e.Date <= lastDay).ToListAsync();
        var expensesByDay = expenses.GroupBy(e => e.Date.Date)
            .ToDictionary(g => g.Key, g => (int)g.Sum(e => e.Amount));

        var allDays = new HashSet<DateTime>();
        foreach (var d in parkingByDay.Keys) allDays.Add(d);
        foreach (var d in washByDay.Keys) allDays.Add(d);
        foreach (var d in monthlyByDay.Keys) allDays.Add(d);
        foreach (var d in expensesByDay.Keys) allDays.Add(d);

        var dailyDetail = allDays.OrderBy(d => d).Select(day =>
        {
            var parking = parkingByDay.GetValueOrDefault(day);
            var wash = washByDay.GetValueOrDefault(day);
            var monthly = monthlyByDay.GetValueOrDefault(day);
            var exp = expensesByDay.GetValueOrDefault(day);
            return new { Day = day.ToString("ddd dd"), Park = parking, Wash = wash, Monthly = monthly,
                Income = parking + wash + monthly, Expenses = exp, Net = parking + wash + monthly - exp };
        }).ToList();

        return Ok(new
        {
            summary = new
            {
                ParkingRevenue = parkingByDay.Values.Sum(),
                WashRevenue = washByDay.Values.Sum(),
                MonthlyRevenue = monthlyByDay.Values.Sum(),
                TotalExpenses = expensesByDay.Values.Sum()
            },
            dailyDetail,
            expenses = expenses.Select(e => new { Date = e.Date.ToString("dd/MM"), e.Category, e.Description,
                Amount = (int)e.Amount }).ToList()
        });
    }
}
