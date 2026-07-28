using System.Text.Json;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using CleanArchitecture.Blazor.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Uso: PrintAgent.exe <serverUrl> [printerName]
// Ejemplo: PrintAgent.exe http://192.168.0.137:5080 "THERMAL Receipt Printer"

var apiUrl = args.Length > 0 ? args[0] : "http://192.168.0.137:5080";
var printerName = args.Length > 1 ? args[1] : "";

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IPrinterService>(sp =>
{
    var env = sp.GetRequiredService<IHostEnvironment>();
    var logger = sp.GetRequiredService<ILogger<PrinterService>>();
    var printer = new PrinterService(env, logger);
    if (!string.IsNullOrWhiteSpace(printerName))
        printer.PrinterName = printerName;
    return printer;
});

builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var printer = host.Services.GetRequiredService<IPrinterService>();

Console.WriteLine("============================================");
Console.WriteLine("  POLIEDRO PARKING - Print Agent v1.0");
Console.WriteLine("============================================");

if (string.IsNullOrWhiteSpace(printer.PrinterName))
{
    Console.WriteLine("\nImpresoras disponibles:");
    try
    {
        var printers = printer.GetInstalledPrinters();
        foreach (var p in printers)
            Console.WriteLine($"  - {p}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  Error al listar: {ex.Message}");
    }
    Console.WriteLine("\nConfigura la impresora antes de iniciar:");
    Console.WriteLine("  PrintAgent.exe <serverUrl> \"<printerName>\"");
    Console.WriteLine("\nEjemplo:");
    Console.WriteLine("  PrintAgent.exe http://192.168.0.137:5080 \"THERMAL Receipt Printer\"");
    return;
}

Console.WriteLine($"\nServidor: {apiUrl}");
Console.WriteLine($"Impresora: {printer.PrinterName}");
Console.WriteLine("Conectando...\n");

var connection = new HubConnectionBuilder()
    .WithUrl($"{apiUrl}/hubs/print")
    .WithAutomaticReconnect()
    .Build();

connection.On<JsonElement>("PrintJob", async job =>
{
    try
    {
        var content = job.GetProperty("content").GetString() ?? "";
        var jobId = job.GetProperty("jobId").GetString() ?? "";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Recibido: {jobId}");

        await printer.PrintTicketAsync(content, CancellationToken.None);
        await connection.InvokeAsync("PrintResult", printer.PrinterName, true, (string?)null);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Impreso: {jobId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ERROR: {ex.Message}");
        try { await connection.InvokeAsync("PrintResult", printer.PrinterName, false, ex.Message); } catch { }
    }
});

connection.Reconnecting += error =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Reconectando...");
    return Task.CompletedTask;
};

connection.Reconnected += async _ =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Reconectado");
};

while (true)
{
    try
    {
        await connection.StartAsync();
        Console.WriteLine("Conectado al servidor. Esperando trabajos...\n");
        await Task.Delay(-1);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error de conexion: {ex.Message}. Reintentando en 5s...");
        await Task.Delay(5000);
    }
}
