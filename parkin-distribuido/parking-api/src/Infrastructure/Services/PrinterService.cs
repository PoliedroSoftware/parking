using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using CleanArchitecture.Blazor.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Blazor.Infrastructure.Services;

public class PrinterService : IPrinterService
{
    private const int TicketColumns = 42;
    private const int SmallTicketColumns = 56;
    private const int DoubleWidthColumns = 21;
    private const int MaxPrintAttempts = 3;
    private const int PrintJobTimeoutSeconds = 30;
    private const int PrintJobPollMilliseconds = 400;
    private const int PrinterSettleMilliseconds = 1200;
    private const int HardSpoolerCleanupAttempts = 3;
    private const int HardSpoolerCleanupTimeoutMilliseconds = 15000;
    private const int ErrorInsufficientBuffer = 122;
    private const int ErrorInvalidParameter = 87;
    private const int ErrorInvalidHandle = 6;
    private const int JobControlDelete = 5;
    private const uint PrinterEnumLocal = 0x00000002;
    private const uint PrinterEnumConnections = 0x00000004;
    private const int PrinterInfoLevel = 4;
    private const int JobInfoLevel = 1;
    private const uint JobStatusPaused = 0x00000001;
    private const uint JobStatusError = 0x00000002;
    private const uint JobStatusDeleting = 0x00000004;
    private const uint JobStatusOffline = 0x00000020;
    private const uint JobStatusPaperOut = 0x00000040;
    private const uint JobStatusPrinted = 0x00000080;
    private const uint JobStatusDeleted = 0x00000100;
    private const uint JobStatusBlockedDeviceQueue = 0x00000200;
    private const uint JobStatusUserIntervention = 0x00000400;
    private const uint JobStatusComplete = 0x00001000;

    private static readonly Encoding PrinterEncoding = CreatePrinterEncoding();

    private readonly string _configPath;
    private readonly ILogger<PrinterService> _logger;
    private readonly SemaphoreSlim _printLock = new(1, 1);
    private readonly object _configLock = new();
    private PrinterConfig _config = new();

    public PrinterService(IHostEnvironment environment, ILogger<PrinterService> logger)
    {
        _logger = logger;
        _configPath = Path.Combine(environment.ContentRootPath, "printer-config.json");
        LoadConfig();
    }

    public string PrinterName
    {
        get
        {
            lock (_configLock)
                return _config.PrinterName;
        }
        set
        {
            lock (_configLock)
            {
                _config.PrinterName = value?.Trim() ?? string.Empty;
                SaveConfig();
            }
        }
    }

    public bool Enabled
    {
        get
        {
            lock (_configLock)
                return _config.Enabled;
        }
        set
        {
            lock (_configLock)
            {
                _config.Enabled = value;
                SaveConfig();
            }
        }
    }

    public IReadOnlyList<string> GetInstalledPrinters()
    {
        if (!OperatingSystem.IsWindows())
            return Array.Empty<string>();

        var requiredBytes = 0;
        var returned = 0;
        _ = EnumPrinters(PrinterEnumLocal | PrinterEnumConnections, null, PrinterInfoLevel, IntPtr.Zero, 0,
            out requiredBytes, out returned);

        if (requiredBytes <= 0)
            return Array.Empty<string>();

        var buffer = Marshal.AllocHGlobal(requiredBytes);
        try
        {
            if (!EnumPrinters(PrinterEnumLocal | PrinterEnumConnections, null, PrinterInfoLevel, buffer,
                    requiredBytes, out requiredBytes, out returned))
            {
                throw CreateWin32Exception("No se pudo consultar las impresoras instaladas.",
                    payloadAccepted: false);
            }

            var printers = new List<string>(returned);
            var size = Marshal.SizeOf<PrinterInfo4>();
            for (var i = 0; i < returned; i++)
            {
                var item = Marshal.PtrToStructure<PrinterInfo4>(IntPtr.Add(buffer, i * size));
                if (!string.IsNullOrWhiteSpace(item.PrinterName))
                    printers.Add(item.PrinterName);
            }

            return printers.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public async Task<int> ClearQueueAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "La limpieza de cola requiere que la aplicacion se ejecute en Windows.");
        }

        string printerName;
        lock (_configLock)
            printerName = _config.PrinterName;

        if (string.IsNullOrWhiteSpace(printerName))
            throw new InvalidOperationException("No se ha configurado una impresora.");

        await _printLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(() => ClearPrinterQueue(printerName), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _printLock.Release();
        }
    }

    public async Task<int> RepairQueueBeforePrintAsync(CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            return 0;

        string printerName;
        lock (_configLock)
            printerName = _config.PrinterName;

        if (string.IsNullOrWhiteSpace(printerName))
            return 0;

        await _printLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return await Task.Run(
                () => RepairPrinterQueueBeforePrint(printerName, cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _printLock.Release();
        }
    }

    public async Task PrintTicketAsync(string ticketContent, CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "La impresion directa requiere que la aplicacion se ejecute en Windows.");
        }

        string printerName;
        lock (_configLock)
            printerName = _config.PrinterName;

        if (string.IsNullOrWhiteSpace(printerName))
            throw new InvalidOperationException("No se ha configurado una impresora.");

        if (string.IsNullOrWhiteSpace(ticketContent))
            throw new InvalidOperationException("El ticket esta vacio.");

        var payload = BuildEscPosTicket(ticketContent);

        await _printLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Task.Run(
                () => SendRawToPrinterWithRetry(printerName, payload, cancellationToken),
                cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _printLock.Release();
        }
    }

    private static byte[] BuildEscPosTicket(string ticketContent)
    {
        var ticket = new EscPosTicket();
        ticket.Initialize();

        var lines = ticketContent.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
            {
                ticket.LineFeed();
                continue;
            }

            if (line == "[HEADER]")
            {
                ticket.Center().Bold(true).Size(0x11).Line("POLIEDRO SOFTWARE");
                ticket.ResetStyles();
                continue;
            }

            if (line == "[DASHED]" || line == "[DIVIDER]")
            {
                ticket.ResetStyles().Left().Line(new string('-', TicketColumns));
                continue;
            }

            if (line == "[DOUBLE]")
            {
                ticket.ResetStyles().Left().Line(new string('=', TicketColumns));
                continue;
            }

            if (line.StartsWith("[QR]:", StringComparison.Ordinal))
            {
                ticket.QrCode(line[5..]);
                continue;
            }

            if (TryWriteTaggedLine(ticket, line, "[HUGE]:", value =>
                    ticket.Center().Bold(true).Size(0x11).Wrapped(value, DoubleWidthColumns)))
                continue;

            if (TryWriteTaggedLine(ticket, line, "[CENTER]:", value =>
                    ticket.Center().Bold(true).Wrapped(value, TicketColumns)))
                continue;

            if (TryWriteTaggedLine(ticket, line, "[CENTER-SMALL]:", value =>
                    ticket.Center().FontB().Wrapped(value, SmallTicketColumns)))
                continue;

            if (TryWriteTaggedLine(ticket, line, "[CENTER-TINY]:", value =>
                    ticket.Center().FontB().Wrapped(value, SmallTicketColumns)))
                continue;

            if (TryWriteRow(ticket, line, "[ROW-BOLD]:", bold: true))
                continue;

            if (TryWriteRow(ticket, line, "[ROW]:", bold: false))
                continue;

            ticket.ResetStyles().Left().Wrapped(line, TicketColumns);
        }

        ticket.ResetStyles().Feed(4).Cut();
        return ticket.ToArray();
    }

    private static Encoding CreatePrinterEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        return Encoding.GetEncoding(850, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
    }

    private static bool TryWriteTaggedLine(EscPosTicket ticket, string line, string tag, Action<string> write)
    {
        if (!line.StartsWith(tag, StringComparison.Ordinal))
            return false;

        var value = line[tag.Length..].Trim();
        write(value);
        ticket.ResetStyles();
        return true;
    }

    private static bool TryWriteRow(EscPosTicket ticket, string line, string tag, bool bold)
    {
        if (!line.StartsWith(tag, StringComparison.Ordinal))
            return false;

        var parts = line[tag.Length..].Split('|', 2);
        var left = parts.Length > 0 ? parts[0].Trim() : string.Empty;
        var right = parts.Length > 1 ? parts[1].Trim() : string.Empty;
        ticket.ResetStyles().Left().Bold(bold).Line(FormatRow(left, right));
        ticket.ResetStyles();
        return true;
    }

    private static string FormatRow(string left, string right)
    {
        left = left.Trim();
        right = right.Trim();

        if (string.IsNullOrEmpty(right))
            return TrimToColumns(left, TicketColumns);

        if (right.Length >= TicketColumns - 1)
            right = TrimToColumns(right, TicketColumns / 2);

        var leftColumns = Math.Max(1, TicketColumns - right.Length - 1);
        left = TrimToColumns(left, leftColumns);

        var spaces = Math.Max(1, TicketColumns - left.Length - right.Length);
        return $"{left}{new string(' ', spaces)}{right}";
    }

    private static string TrimToColumns(string value, int columns)
    {
        if (value.Length <= columns)
            return value;

        return value[..Math.Max(0, columns)];
    }

    private static int SendRawToPrinter(string printerName, byte[] payload)
    {
        var blockingJobs = GetPrinterJobs(printerName);
        if (blockingJobs.Count > 0)
        {
            throw new PrintSpoolerException(
                "La impresora tiene trabajos pendientes antes del ticket: " +
                $"{FormatBlockingJobs(blockingJobs)}. Limpie la cola e intente de nuevo.",
                nativeErrorCode: 0,
                payloadAccepted: false);
        }

        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
            throw CreateWin32Exception($"No se pudo abrir la impresora '{printerName}'.", payloadAccepted: false);

        try
        {
            var docInfo = new DocInfo1
            {
                DocumentName = "Poliedro ticket",
                OutputFile = null,
                DataType = "RAW"
            };

            var jobId = StartDocPrinter(printerHandle, 1, ref docInfo);
            if (jobId == 0)
            {
                throw CreateWin32Exception(
                    $"No se pudo iniciar el trabajo de impresion en '{printerName}'.",
                    payloadAccepted: false);
            }

            try
            {
                if (!StartPagePrinter(printerHandle))
                {
                    throw CreateWin32Exception(
                        $"No se pudo iniciar la pagina en '{printerName}'.",
                        payloadAccepted: false);
                }

                try
                {
                    if (!WritePrinter(printerHandle, payload, payload.Length, out var bytesWritten) ||
                        bytesWritten != payload.Length)
                    {
                        throw CreateWin32Exception(
                            $"No se pudo enviar el ticket completo a '{printerName}'.",
                            payloadAccepted: bytesWritten > 0);
                    }
                }
                finally
                {
                    _ = EndPagePrinter(printerHandle);
                }
            }
            finally
            {
                _ = EndDocPrinter(printerHandle);
            }

            return jobId;
        }
        finally
        {
            _ = ClosePrinter(printerHandle);
        }
    }

    private static int ClearPrinterQueue(string printerName)
    {
        var jobs = GetPrinterJobs(printerName);
        if (jobs.Count == 0)
            return 0;

        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
        {
            throw CreateWin32Exception($"No se pudo abrir la impresora '{printerName}' para limpiar la cola.",
                payloadAccepted: false);
        }

        try
        {
            var deleted = 0;
            foreach (var job in jobs)
            {
                if (SetJob(printerHandle, job.JobId, 0, IntPtr.Zero, JobControlDelete))
                    deleted++;
            }

            return deleted;
        }
        finally
        {
            _ = ClosePrinter(printerHandle);
        }
    }

    private void SendRawToPrinterWithRetry(string printerName, byte[] payload, CancellationToken cancellationToken)
    {
        PrintSpoolerException? lastError = null;

        for (var attempt = 1; attempt <= MaxPrintAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _ = RepairPrinterQueueBeforePrint(printerName, cancellationToken);
                var jobId = SendRawToPrinter(printerName, payload);
                WaitForPrintJobToFinish(printerName, jobId, cancellationToken);
                Thread.Sleep(PrinterSettleMilliseconds);
                return;
            }
            catch (PrintSpoolerException ex) when (!ex.PayloadAccepted && attempt < MaxPrintAttempts)
            {
                lastError = ex;
                _logger.LogWarning(ex, "Intento {Attempt} de impresion fallido antes de enviar el ticket.",
                    attempt);
                Thread.Sleep(TimeSpan.FromMilliseconds(650 * attempt));
            }
        }

        if (lastError is not null)
        {
            throw new InvalidOperationException(
                $"No se pudo enviar el ticket a '{printerName}' despues de {MaxPrintAttempts} intentos. " +
                lastError.Message,
                lastError);
        }
    }

    private int RepairPrinterQueueBeforePrint(string printerName, CancellationToken cancellationToken)
    {
        IReadOnlyList<PrintJobSnapshot> jobs;
        try
        {
            jobs = GetPrinterJobs(printerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo leer la cola de '{PrinterName}'. Se intentara reparar spooler.",
                printerName);
            var repairError = RunHardSpoolerCleanupAttempts(printerName, cancellationToken, ex.Message);
            if (!string.IsNullOrWhiteSpace(repairError))
            {
                _logger.LogWarning(
                    "No se pudo reparar la cola de '{PrinterName}' despues de {Attempts} intentos. Detalle: {Error}",
                    printerName, HardSpoolerCleanupAttempts, repairError);
                return -1;
            }

            return 0;
        }

        if (jobs.Count == 0)
            return 0;

        _logger.LogWarning("La cola de '{PrinterName}' tiene {JobCount} trabajo(s). Se intentara limpiar.",
            printerName, jobs.Count);

        var affectedJobs = jobs.Count;
        try
        {
            _ = ClearPrinterQueue(printerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo limpiar la cola de '{PrinterName}' usando Winspool. Se intentara limpieza fuerte.",
                printerName);
        }

        Thread.Sleep(500);

        try
        {
            jobs = GetPrinterJobs(printerName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo releer la cola de '{PrinterName}' despues de limpieza normal.",
                printerName);
        }

        if (jobs.Count == 0)
            return affectedJobs;

        var lastCleanupError = RunHardSpoolerCleanupAttempts(printerName, cancellationToken, null);
        if (string.IsNullOrWhiteSpace(lastCleanupError))
            return affectedJobs;

        _logger.LogWarning(
            "No se pudo limpiar la cola de '{PrinterName}' despues de {Attempts} intentos. Detalle: {Error}",
            printerName, HardSpoolerCleanupAttempts, lastCleanupError);
        return -1;
    }

    private string? RunHardSpoolerCleanupAttempts(
        string printerName,
        CancellationToken cancellationToken,
        string? initialError)
    {
        var lastCleanupError = initialError;
        for (var attempt = 1; attempt <= HardSpoolerCleanupAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogWarning("Limpieza fuerte de spooler intento {Attempt}/{MaxAttempts}.",
                attempt, HardSpoolerCleanupAttempts);

            lastCleanupError = RunHardSpoolerCleanup(attempt);
            Thread.Sleep(1000);

            try
            {
                var jobs = GetPrinterJobs(printerName);
                if (jobs.Count == 0)
                    return null;
            }
            catch (Exception ex)
            {
                lastCleanupError = ex.Message;
                continue;
            }
        }

        return lastCleanupError;
    }

    private static string? RunHardSpoolerCleanup(int attempt)
    {
        const string script =
            "Stop-Service spooler -Force; " +
            "Remove-Item \"C:\\Windows\\System32\\spool\\PRINTERS\\*\" -Force -ErrorAction SilentlyContinue; " +
            "Start-Service spooler";

        var encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        process.Start();
        if (!process.WaitForExit(HardSpoolerCleanupTimeoutMilliseconds))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Best effort cleanup for a timed out PowerShell process.
            }

            return $"La limpieza fuerte de spooler intento {attempt} excedio el tiempo limite.";
        }

        if (process.ExitCode == 0)
            return null;

        var error = process.StandardError.ReadToEnd();
        if (string.IsNullOrWhiteSpace(error))
            error = process.StandardOutput.ReadToEnd();

        return
            "Windows no permitio limpiar el spooler. " +
            "Ejecute la aplicacion como administrador. " +
            $"Detalle: {CleanPowerShellOutput(error)}";
    }

    private static string CleanPowerShellOutput(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "sin detalle";

        var cleaned = value
            .Replace("_x000D__x000A_", " ", StringComparison.OrdinalIgnoreCase)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        var clixmlIndex = cleaned.IndexOf("#< CLIXML", StringComparison.OrdinalIgnoreCase);
        if (clixmlIndex >= 0)
            cleaned = cleaned[..clixmlIndex].Trim();

        return string.IsNullOrWhiteSpace(cleaned)
            ? "PowerShell no pudo detener o iniciar el servicio spooler."
            : cleaned;
    }

    private static IReadOnlyList<PrintJobSnapshot> GetPrinterJobs(string printerName)
    {
        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
        {
            throw CreateWin32Exception($"No se pudo abrir la impresora '{printerName}' para revisar la cola.",
                payloadAccepted: false);
        }

        try
        {
            var requiredBytes = 0;
            var returned = 0;
            _ = EnumJobs(printerHandle, 0, 99, JobInfoLevel, IntPtr.Zero, 0, out requiredBytes, out returned);
            var error = Marshal.GetLastWin32Error();

            if (requiredBytes <= 0)
                return Array.Empty<PrintJobSnapshot>();

            if (error != ErrorInsufficientBuffer && error != 0)
            {
                throw CreateWin32Exception($"No se pudo consultar la cola de '{printerName}'.",
                    payloadAccepted: false);
            }

            var buffer = Marshal.AllocHGlobal(requiredBytes);
            try
            {
                if (!EnumJobs(printerHandle, 0, 99, JobInfoLevel, buffer, requiredBytes, out requiredBytes,
                        out returned))
                {
                    throw CreateWin32Exception($"No se pudo consultar la cola de '{printerName}'.",
                        payloadAccepted: false);
                }

                var jobs = new List<PrintJobSnapshot>(returned);
                var size = Marshal.SizeOf<JobInfo1>();
                for (var i = 0; i < returned; i++)
                {
                    var job = Marshal.PtrToStructure<JobInfo1>(IntPtr.Add(buffer, i * size));
                    jobs.Add(new PrintJobSnapshot(
                        (int)job.JobId,
                        Marshal.PtrToStringUni(job.Document) ?? "Sin nombre",
                        job.Status));
                }

                return jobs;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            _ = ClosePrinter(printerHandle);
        }
    }

    private static string FormatBlockingJobs(IReadOnlyList<PrintJobSnapshot> jobs)
    {
        var formattedJobs = jobs
            .Take(3)
            .Select(job => $"#{job.JobId} {job.Document} ({DescribeJobStatus(job.Status)})");

        var text = string.Join("; ", formattedJobs);
        return jobs.Count <= 3 ? text : $"{text}; y {jobs.Count - 3} mas";
    }

    private static void WaitForPrintJobToFinish(
        string printerName,
        int jobId,
        CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(PrintJobTimeoutSeconds))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobStatus = GetPrintJobStatus(printerName, jobId);
            if (jobStatus is null || IsSuccessfulJobStatus(jobStatus.Value))
                return;

            if (IsFailedJobStatus(jobStatus.Value))
            {
                TryDeletePrintJob(printerName, jobId);
                throw new PrintSpoolerException(
                    $"El trabajo de impresion {jobId} quedo en error: {DescribeJobStatus(jobStatus.Value)}.",
                    nativeErrorCode: 0,
                    payloadAccepted: true);
            }

            Thread.Sleep(PrintJobPollMilliseconds);
        }

        TryDeletePrintJob(printerName, jobId);
        throw new PrintSpoolerException(
            $"El trabajo de impresion {jobId} no salio de la cola de '{printerName}' en " +
            $"{PrintJobTimeoutSeconds} segundos. Revise papel, tapa, conexion USB/red y cola de impresion.",
            nativeErrorCode: 0,
            payloadAccepted: true);
    }

    private static uint? GetPrintJobStatus(string printerName, int jobId)
    {
        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
            throw CreateWin32Exception($"No se pudo abrir la impresora '{printerName}' para revisar la cola.",
                payloadAccepted: true);

        try
        {
            var requiredBytes = 0;
            _ = GetJob(printerHandle, jobId, JobInfoLevel, IntPtr.Zero, 0, out requiredBytes);
            var error = Marshal.GetLastWin32Error();

            if (error is ErrorInvalidParameter or ErrorInvalidHandle)
                return null;

            if (error != ErrorInsufficientBuffer || requiredBytes <= 0)
            {
                if (error == 0)
                    return null;

                throw CreateWin32Exception($"No se pudo leer el estado del trabajo de impresion {jobId}.",
                    payloadAccepted: true);
            }

            var buffer = Marshal.AllocHGlobal(requiredBytes);
            try
            {
                if (!GetJob(printerHandle, jobId, JobInfoLevel, buffer, requiredBytes, out requiredBytes))
                {
                    error = Marshal.GetLastWin32Error();
                    if (error is ErrorInvalidParameter or ErrorInvalidHandle)
                        return null;

                    throw CreateWin32Exception($"No se pudo leer el estado del trabajo de impresion {jobId}.",
                        payloadAccepted: true);
                }

                var jobInfo = Marshal.PtrToStructure<JobInfo1>(buffer);
                return jobInfo.Status;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            _ = ClosePrinter(printerHandle);
        }
    }

    private static bool IsSuccessfulJobStatus(uint status)
    {
        if ((status & (JobStatusPrinted | JobStatusDeleted | JobStatusComplete)) != 0)
            return true;

        var fatalStatus = JobStatusError | JobStatusOffline | JobStatusPaperOut |
                          JobStatusBlockedDeviceQueue | JobStatusUserIntervention;
        return (status & JobStatusDeleting) != 0 && (status & fatalStatus) == 0;
    }

    private static bool IsFailedJobStatus(uint status) =>
        (status & (JobStatusPaused | JobStatusError | JobStatusOffline | JobStatusPaperOut |
                   JobStatusBlockedDeviceQueue | JobStatusUserIntervention)) != 0;

    private static string DescribeJobStatus(uint status)
    {
        var states = new List<string>();
        if ((status & JobStatusPaused) != 0) states.Add("pausado");
        if ((status & JobStatusError) != 0) states.Add("error");
        if ((status & JobStatusDeleting) != 0) states.Add("eliminando");
        if ((status & JobStatusOffline) != 0) states.Add("offline");
        if ((status & JobStatusPaperOut) != 0) states.Add("sin papel");
        if ((status & JobStatusBlockedDeviceQueue) != 0) states.Add("cola bloqueada");
        if ((status & JobStatusUserIntervention) != 0) states.Add("requiere intervencion");

        return states.Count == 0 ? $"estado desconocido ({status})" : string.Join(", ", states);
    }

    private static void TryDeletePrintJob(string printerName, int jobId)
    {
        if (!OpenPrinter(printerName, out var printerHandle, IntPtr.Zero))
            return;

        try
        {
            _ = SetJob(printerHandle, jobId, 0, IntPtr.Zero, JobControlDelete);
        }
        finally
        {
            _ = ClosePrinter(printerHandle);
        }
    }

    private static PrintSpoolerException CreateWin32Exception(string message, bool payloadAccepted)
    {
        var error = Marshal.GetLastWin32Error();
        var details = error == 0 ? "Error desconocido." : new Win32Exception(error).Message;
        return new PrintSpoolerException($"{message} Detalle Windows: {details}", error, payloadAccepted);
    }

    private void LoadConfig()
    {
        try
        {
            if (!File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            _config = JsonSerializer.Deserialize<PrinterConfig>(json) ?? new PrinterConfig();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar la configuracion de impresora.");
            _config = new PrinterConfig();
        }
    }

    private void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar la configuracion de impresora.");
        }
    }

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool EnumPrinters(
        uint flags,
        string? name,
        int level,
        IntPtr printerEnum,
        int bufferSize,
        out int requiredBytes,
        out int returned);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printerHandle, IntPtr defaults);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int StartDocPrinter(IntPtr printerHandle, int level, ref DocInfo1 docInfo);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(
        IntPtr printerHandle,
        byte[] bytes,
        int byteCount,
        out int bytesWritten);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr printerHandle);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetJob(
        IntPtr printerHandle,
        int jobId,
        int level,
        IntPtr job,
        int bufferSize,
        out int requiredBytes);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool EnumJobs(
        IntPtr printerHandle,
        int firstJob,
        int numberOfJobs,
        int level,
        IntPtr job,
        int bufferSize,
        out int requiredBytes,
        out int returned);

    [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool SetJob(
        IntPtr printerHandle,
        int jobId,
        int level,
        IntPtr job,
        int command);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PrinterInfo4
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? PrinterName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? ServerName;

        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DocInfo1
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string DocumentName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? OutputFile;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string DataType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct JobInfo1
    {
        public uint JobId;
        public IntPtr PrinterName;
        public IntPtr MachineName;
        public IntPtr UserName;
        public IntPtr Document;
        public IntPtr Datatype;
        public IntPtr StatusText;
        public uint Status;
        public uint Priority;
        public uint Position;
        public uint TotalPages;
        public uint PagesPrinted;
        public SystemTime Submitted;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Milliseconds;
    }

    private sealed class PrintSpoolerException : InvalidOperationException
    {
        public PrintSpoolerException(string message, int nativeErrorCode, bool payloadAccepted)
            : base(message)
        {
            NativeErrorCode = nativeErrorCode;
            PayloadAccepted = payloadAccepted;
        }

        public int NativeErrorCode { get; }
        public bool PayloadAccepted { get; }
    }

    private sealed record PrintJobSnapshot(int JobId, string Document, uint Status);

    private sealed class EscPosTicket
    {
        private readonly MemoryStream _buffer = new();

        public EscPosTicket Initialize()
        {
            Write(0x1B, 0x40);
            Write(0x1B, 0x74, 0x02);
            return this;
        }

        public EscPosTicket ResetStyles() => Left().Bold(false).Reverse(false).Size(0x00).FontA();

        public EscPosTicket Left()
        {
            Write(0x1B, 0x61, 0x00);
            return this;
        }

        public EscPosTicket Center()
        {
            Write(0x1B, 0x61, 0x01);
            return this;
        }

        public EscPosTicket Bold(bool enabled)
        {
            Write(0x1B, 0x45, enabled ? (byte)0x01 : (byte)0x00);
            return this;
        }

        public EscPosTicket Reverse(bool enabled)
        {
            Write(0x1D, 0x42, enabled ? (byte)0x01 : (byte)0x00);
            return this;
        }

        public EscPosTicket FontA()
        {
            Write(0x1B, 0x4D, 0x00);
            return this;
        }

        public EscPosTicket FontB()
        {
            Write(0x1B, 0x4D, 0x01);
            return this;
        }

        public EscPosTicket Size(byte size)
        {
            Write(0x1D, 0x21, size);
            return this;
        }

        public EscPosTicket Wrapped(string value, int columns)
        {
            foreach (var line in WrapText(value, columns))
                Line(line);

            return this;
        }

        public EscPosTicket Line(string value)
        {
            var bytes = PrinterEncoding.GetBytes(value);
            _buffer.Write(bytes, 0, bytes.Length);
            LineFeed();
            return this;
        }

        public EscPosTicket LineFeed()
        {
            _buffer.WriteByte(0x0A);
            return this;
        }

        public EscPosTicket Feed(byte lines)
        {
            Write(0x1B, 0x64, lines);
            return this;
        }

        public EscPosTicket QrCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return this;

            var data = Encoding.UTF8.GetBytes(value.Trim());
            var storeLength = data.Length + 3;
            Center();
            Write(0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x32, 0x00);
            Write(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x04);
            Write(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x31);
            Write(0x1D, 0x28, 0x6B, (byte)(storeLength & 0xFF), (byte)(storeLength >> 8), 0x31, 0x50, 0x30);
            _buffer.Write(data, 0, data.Length);
            Write(0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30);
            LineFeed();
            return this;
        }

        public EscPosTicket Cut()
        {
            Write(0x1D, 0x56, 0x00);
            return this;
        }

        public byte[] ToArray() => _buffer.ToArray();

        private static IEnumerable<string> WrapText(string value, int columns)
        {
            value = value.Trim();
            if (string.IsNullOrEmpty(value))
            {
                yield return string.Empty;
                yield break;
            }

            while (value.Length > columns)
            {
                var splitIndex = value.LastIndexOf(' ', Math.Min(columns, value.Length - 1));
                if (splitIndex <= 0)
                    splitIndex = columns;

                yield return value[..splitIndex].TrimEnd();
                value = value[splitIndex..].TrimStart();
            }

            yield return value;
        }

        private void Write(params byte[] bytes) => _buffer.Write(bytes, 0, bytes.Length);
    }

    private sealed class PrinterConfig
    {
        public string PrinterName { get; set; } = string.Empty;
        public bool Enabled { get; set; }
    }
}
