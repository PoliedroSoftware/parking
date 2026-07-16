// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using CleanArchitecture.Blazor.Domain.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CleanArchitecture.Blazor.Application.Common.Interfaces;

public interface IApplicationDbContext : IAsyncDisposable
{


    DbSet<Carpark> Carparks { get; set; }
    DbSet<Zone> Zones { get; set; }
    DbSet<Gate> Gates { get; set; }
    DbSet<Charge> Charges { get; set; }
    DbSet<Vehicle> Vehicles { get; set; }
    DbSet<Member> Members { get; set; }
    DbSet<SpaceGroup> SpaceGroups { get; set; }
    DbSet<MemberRental> MemberRentals { get; set; }
    DbSet<MemberVehicle> MemberVehicles { get; set; }

    DbSet<CarWash> CarWashes { get; set; }
    DbSet<CarWashAdditional> CarWashAdditionals { get; set; }
    DbSet<CarWashOperator> CarWashOperators { get; set; }
    DbSet<WashServicePrice> WashServicePrices { get; set; }
    DbSet<WashAdditional> WashAdditionals { get; set; }
    DbSet<Expense> Expenses { get; set; }
    DbSet<ParkingRecord> ParkingRecords { get; set; }

    DbSet<Holiday> Holidays { get; set; }

    DbSet<SystemLog> SystemLogs { get; set; }
    DbSet<AuditTrail> AuditTrails { get; set; }

    DbSet<PicklistSet> PicklistSets { get; set; }

    DbSet<Tenant> Tenants { get; set; }
    DbSet<TenantUser> TenantUsers { get; set; }

    DbSet<LoginAudit> LoginAudits { get; set; }
    DbSet<UserLoginRiskSummary> UserLoginRiskSummaries { get; set; }
    ChangeTracker ChangeTracker { get; }

    DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
