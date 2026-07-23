using System.Collections.Concurrent;

namespace CleanArchitecture.Blazor.Server.UI.Services;

public class MovementRecord
{
    public int RecordId { get; set; }
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int GrossAmount { get; set; }
    public int DiscountAmount { get; set; }
    public string Status { get; set; } = "Activo";
    public DateTime? ExitTime { get; set; }
    public string FormattedDuration { get; set; } = string.Empty;
}

public static class MovementTracker
{
    private static readonly ConcurrentBag<MovementRecord> _allMovements = new();

    public static void AddMovement(MovementRecord movement)
    {
        _allMovements.Add(movement);
    }

    public static MovementRecord? UpdateExit(string licensePlate, string customerName, int amount, string formattedDuration)
    {
        var record = _allMovements
            .Where(m => m.LicensePlate == licensePlate && m.Type == "Entrada" && m.Status == "Activo")
            .OrderByDescending(m => m.Time)
            .FirstOrDefault();

        if (record != null)
        {
            record.Status = "Completado";
            record.ExitTime = DateTime.Now;
            record.CustomerName = customerName;
            record.Amount = amount;
            record.FormattedDuration = formattedDuration;
        }
        return record;
    }

    public static IEnumerable<MovementRecord> GetActiveEntries()
    {
        return _allMovements.Where(m => m.Type == "Entrada" && m.Status == "Activo")
            .OrderBy(m => m.Time);
    }

    public static IEnumerable<MovementRecord> GetTodayMovements()
    {
        return _allMovements.Where(m => m.Time.Date == DateTime.Today)
            .OrderByDescending(m => m.ExitTime ?? m.Time);
    }

    public static IEnumerable<MovementRecord> GetAllMovements()
    {
        return _allMovements.OrderByDescending(m => m.ExitTime ?? m.Time);
    }

    public static DaySummary GetDaySummary(DateTime? date = null)
    {
        var target = date?.Date ?? DateTime.Today;
        var movements = _allMovements.Where(m => m.Time.Date == target).ToList();

        return new DaySummary
        {
            Date = target,
            TotalEntries = movements.Count(m => m.Type == "Entrada"),
            TotalExits = movements.Count(m => m is { Type: "Entrada", Status: "Completado" }),
            TotalWashes = movements.Count(m => m.Type == "Lavado"),
            TotalMonthly = movements.Count(m => m.Type == "Mensualidad"),
            TotalParkingRevenue = movements.Where(m => m.Type == "Entrada" && m.Status == "Completado").Sum(m => m.Amount),
            TotalWashRevenue = movements.Where(m => m.Type == "Lavado").Sum(m => m.Amount),
            TotalMonthlyRevenue = movements.Where(m => m.Type == "Mensualidad").Sum(m => m.Amount),
            Movements = movements.OrderByDescending(m => m.ExitTime ?? m.Time).ToList()
        };
    }
}

public class DaySummary
{
    public DateTime Date { get; set; }
    public int TotalEntries { get; set; }
    public int TotalExits { get; set; }
    public int TotalWashes { get; set; }
    public int TotalMonthly { get; set; }
    public int TotalParkingRevenue { get; set; }
    public int TotalWashRevenue { get; set; }
    public int TotalMonthlyRevenue { get; set; }
    public int TotalRevenue => TotalParkingRevenue + TotalWashRevenue + TotalMonthlyRevenue;
    public List<MovementRecord> Movements { get; set; } = new();
}
