// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using CleanArchitecture.Blazor.Domain.Common.Entities;
using CleanArchitecture.Blazor.Domain.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence;

#nullable disable
public class ApplicationDbContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, string,
    ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin,
    ApplicationRoleClaim, ApplicationUserToken>, IApplicationDbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Carpark> Carparks { get; set; }
    public DbSet<Zone> Zones { get; set; }
    public DbSet<Gate> Gates { get; set; }
    public DbSet<Charge> Charges { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<SpaceGroup> SpaceGroups { get; set; }
    public DbSet<MemberRental> MemberRentals { get; set; }
    public DbSet<MemberVehicle> MemberVehicles { get; set; }

    public DbSet<CarWash> CarWashes { get; set; }
    public DbSet<CarWashAdditional> CarWashAdditionals { get; set; }
    public DbSet<CarWashOperator> CarWashOperators { get; set; }
    public DbSet<WashServicePrice> WashServicePrices { get; set; }
    public DbSet<WashAdditional> WashAdditionals { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<ParkingRecord> ParkingRecords { get; set; }
    public DbSet<ParkingRate> ParkingRates { get; set; }
    public DbSet<MonthlyRate> MonthlyRates { get; set; }

    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }
  

    public DbSet<PicklistSet> PicklistSets { get; set; }
 
    public DbSet<LoginAudit> LoginAudits { get; set; }
    public DbSet<UserLoginRiskSummary> UserLoginRiskSummaries { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplyGlobalFilters<ISoftDelete>(s => s.DeletedAt == null);
    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<string>().HaveMaxLength(450);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
}
