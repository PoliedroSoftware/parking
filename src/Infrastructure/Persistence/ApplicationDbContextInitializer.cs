using System;
using System.Reflection;
using CleanArchitecture.Blazor.Application.Common.Constants;
using CleanArchitecture.Blazor.Application.Common.Security;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Common;
using CleanArchitecture.Blazor.Domain.Constants;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = dbContextFactory.CreateDbContext();
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
                await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedTenantsAsync();
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedDataAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static IEnumerable<string> GetAllPermissions()
    {
        var allPermissions = new List<string>();
        var modules = typeof(Permissions).GetNestedTypes();

        foreach (var module in modules)
        {
            var moduleName = string.Empty;
            var moduleDescription = string.Empty;

            var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var fi in fields)
            {
                var propertyValue = fi.GetValue(null);

                if (propertyValue is not null)
                    allPermissions.Add((string)propertyValue);
            }
        }

        return allPermissions;
    }




    private async Task SeedTenantsAsync()
    {
        if (await _context.Tenants.AnyAsync()) return;

        _logger.LogInformation("Seeding organizations...");
        var tenants = new[]
        {
                new Tenant { Name = "Main", Description = "Main Site" },
                new Tenant { Name = "Europe", Description = "Europe Site" }
            };

        await _context.Tenants.AddRangeAsync(tenants);
        await _context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        var adminRoleName = Roles.Admin;
        var userRoleName = Roles.Basic;

        var adminRole = await _roleManager.FindByNameAsync(adminRoleName);
        if (adminRole == null)
        {
            _logger.LogInformation("Seeding roles...");
            adminRole = new ApplicationRole(adminRoleName)
            {
                Description = "Admin Group",
                CreatedAt = DateTime.UtcNow,
            };
            await _roleManager.CreateAsync(adminRole);

            var userRole = new ApplicationRole(userRoleName)
            {
                Description = "Basic Group",
                CreatedAt = DateTime.UtcNow,
            };
            await _roleManager.CreateAsync(userRole);
        }

        var permissions = GetAllPermissions();
        var existingClaims = await _roleManager.GetClaimsAsync(adminRole);
        var existingClaimValues = existingClaims.Where(c => c.Type == ApplicationClaimTypes.Permission).Select(c => c.Value).ToHashSet();

        foreach (var permission in permissions)
        {
            if (!existingClaimValues.Contains(permission))
            {
                var claim = new Claim(ApplicationClaimTypes.Permission, permission);
                await _roleManager.AddClaimAsync(adminRole, claim);
                _logger.LogInformation("Added new permission to admin role: {Permission}", permission);
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync()) return;

        _logger.LogInformation("Seeding users...");
        var tenants = await _context.Tenants.ToListAsync();
        var adminUser = new ApplicationUser
        {
            UserName = Users.Administrator,
            Provider = "Local",
            IsActive = true,
            TenantId = (await _context.Tenants.FirstAsync()).Id,
            DisplayName = Users.Administrator,
            Email = "admin@example.com",
            EmailConfirmed = true,
            ProfilePictureDataUrl = "https://s.gravatar.com/avatar/78be68221020124c23c665ac54e07074?s=80",
            LanguageCode="en-US",
            TimeZoneId= "Asia/Shanghai",
            TwoFactorEnabled = false,
            CreatedAt=DateTime.UtcNow,
            TenantUsers = tenants.Select(t => new TenantUser { TenantId = t.Id }).ToList()
        };
        await _userManager.CreateAsync(adminUser, Users.DefaultPassword);
        await _userManager.AddToRoleAsync(adminUser, Roles.Admin);
        var demoUser = new ApplicationUser
        {
            UserName = Users.Demo,
            IsActive = true,
            Provider = "Local",
            TenantId = (await _context.Tenants.FirstAsync()).Id,
            DisplayName = Users.Demo,
            SuperiorId = adminUser.Id,
            Email = "demo@example.com",
            EmailConfirmed = true,
            LanguageCode = "de-DE",
            TimeZoneId = "Europe/Berlin",
            TenantUsers = new List<TenantUser> { new TenantUser { TenantId = tenants.First().Id } },
            ProfilePictureDataUrl = "https://s.gravatar.com/avatar/ea753b0b0f357a41491408307ade445e?s=80",
            CreatedAt = DateTime.UtcNow
        };

       

        await _userManager.CreateAsync(demoUser, Users.DefaultPassword);
        await _userManager.AddToRoleAsync(demoUser, Roles.Basic);
    }

    private async Task SeedDataAsync()
    {
        var tenant = await _context.Tenants.FirstAsync();
        // 1. Datos basicos de diccionario (solo primera inicializacion)
        if (!await _context.PicklistSets.AnyAsync())
        {
            _logger.LogInformation("Seeding picklist sets...");
            var keyValues = new[]
            {
                new PicklistSet { Name = Picklist.Status, Value = "initialization", Text = "Initialization", Description = "Status of workflow" },
                new PicklistSet { Name = Picklist.Status, Value = "processing",     Text = "Processing",     Description = "Status of workflow" },
                new PicklistSet { Name = Picklist.Status, Value = "pending",        Text = "Pending",        Description = "Status of workflow" },
                new PicklistSet { Name = Picklist.Status, Value = "done",           Text = "Done",           Description = "Status of workflow" },
                new PicklistSet { Name = Picklist.Brand,  Value = "Apple",          Text = "Apple",          Description = "Brand of production" },
                new PicklistSet { Name = Picklist.Brand,  Value = "Google",         Text = "Google",         Description = "Brand of production" },
                new PicklistSet { Name = Picklist.Brand,  Value = "Microsoft",      Text = "Microsoft",      Description = "Brand of production" },
                new PicklistSet { Name = Picklist.Unit,   Value = "EA",             Text = "EA",             Description = "Unit of product" },
                new PicklistSet { Name = Picklist.Unit,   Value = "KM",             Text = "KM",             Description = "Unit of product" },
                new PicklistSet { Name = Picklist.Unit,   Value = "PC",             Text = "PC",             Description = "Unit of product" },
                new PicklistSet { Name = Picklist.Unit,   Value = "L",              Text = "L",              Description = "Unit of product" },
            };
            await _context.PicklistSets.AddRangeAsync(keyValues);
            await _context.SaveChangesAsync();
        }

        // 2. Planes de tarifa (Charges)
        if (!await _context.Charges.AnyAsync())
        {
            _logger.LogInformation("Seeding charges (rate plans)...");
            var standardRate = new Charge
            {
                Name = "Tarifa Horaria Estandar",
                EffectiveDate = DateTime.UtcNow.Date.AddYears(1),
                Description = "Standard hourly parking rate with peak / night preferences",
                BeforeContent = new RateContent
                {
                    NormalCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 12, PriceHoliday = 15 },
                        new ChargeItem { Duration = 30, PriceWeekday = 12, PriceHoliday = 15 },
                        new ChargeItem { Duration = 60, PriceWeekday = 22, PriceHoliday = 26 },
                    ],
                    SpecialPeriod =
                    [
                        new ChargePeriod { StartTime = new TimeSpan(7,30,0), EndTime = new TimeSpan(23, 0, 0) } // Morning Peak
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 18, PriceHoliday = 20 },
                        new ChargeItem { Duration = 60, PriceWeekday = 32, PriceHoliday = 36 },
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(0, 0, 0), EndTime = new TimeSpan(8, 0, 0) } ],
                        CeilingAmountWeekday = 120,
                        CeilingAmountHoliday = 140
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(20, 0, 0), EndTime = new TimeSpan(8, 0, 0) } ],
                        CeilingAmountWeekday = 60,
                        CeilingAmountHoliday = 70
                    },
                    Max12Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 150, CeilingAmountHoliday = 180 },
                    Max24Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 230, CeilingAmountHoliday = 260 },
                    FullDayPark = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 200, CeilingAmountHoliday = 240 }
                },
                AfterContent = new RateContent
                {
                    NormalCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 11, PriceHoliday = 13 },
                        new ChargeItem { Duration = 60, PriceWeekday = 19, PriceHoliday = 23 },
                        new ChargeItem { Duration = 120, PriceWeekday = 32, PriceHoliday = 38 }
                    ],
                    SpecialPeriod =
                    [
                        new ChargePeriod { StartTime = new TimeSpan(20,0,0), EndTime = new TimeSpan(23, 0, 0) }
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 10, PriceHoliday = 12 },
                        new ChargeItem { Duration = 60, PriceWeekday = 17, PriceHoliday = 21 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [new ChargePeriod { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0) }],
                        CeilingAmountWeekday = 115,
                        CeilingAmountHoliday = 135
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [new ChargePeriod { StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(7, 0, 0) }],
                        CeilingAmountWeekday = 42,
                        CeilingAmountHoliday = 52
                    },
                    Max12Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 145, CeilingAmountHoliday = 165 },
                    Max24Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 185, CeilingAmountHoliday = 205 },
                    FullDayPark = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 175, CeilingAmountHoliday = 195 }
                }
            };

            var evFriendlyRate = new Charge
            {
                Name = "Tarifa Electrica",
                EffectiveDate = DateTime.UtcNow.Date.AddDays(90),
                Description = "Discounted evening rate for EV vehicles",
                BeforeContent = new RateContent
                {
                    NormalCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 10, PriceHoliday = 12 },
                        new ChargeItem { Duration = 60, PriceWeekday = 18, PriceHoliday = 22 }
                    ],
                    SpecialPeriod =
                    [
                        new ChargePeriod { StartTime = new TimeSpan(18,0,0), EndTime = new TimeSpan(23, 0, 0) } // Evening promo period
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 9, PriceHoliday = 11 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0) } ],
                        CeilingAmountWeekday = 110,
                        CeilingAmountHoliday = 130
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(18,0,0), EndTime = new TimeSpan(7, 0, 0) } ],
                        CeilingAmountWeekday = 40,
                        CeilingAmountHoliday = 50
                    },
                    Max12Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 140, CeilingAmountHoliday = 160 },
                    Max24Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 180, CeilingAmountHoliday = 200 },
                    FullDayPark = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 170, CeilingAmountHoliday = 190 }
                },
                AfterContent = new RateContent
                {
                    NormalCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 11, PriceHoliday = 13 },
                        new ChargeItem { Duration = 60, PriceWeekday = 19, PriceHoliday = 23 },
                        new ChargeItem { Duration = 120, PriceWeekday = 32, PriceHoliday = 38 }
                    ],
                    SpecialPeriod =
                    [
                        new ChargePeriod { StartTime = new TimeSpan(1,0,0), EndTime = new TimeSpan(8,0,0) }
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 10, PriceHoliday = 12 },
                        new ChargeItem { Duration = 60, PriceWeekday = 17, PriceHoliday = 21 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0) } ],
                        CeilingAmountWeekday = 115,
                        CeilingAmountHoliday = 135
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(23, 0, 0) } ],
                        CeilingAmountWeekday = 42,
                        CeilingAmountHoliday = 52
                    },
                    Max12Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 145, CeilingAmountHoliday = 165 },
                    Max24Park = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 185, CeilingAmountHoliday = 205 },
                    FullDayPark = new MaxReducedItem { IsActive = true, CeilingAmountWeekday = 175, CeilingAmountHoliday = 195 }
                }
            };

            await _context.Charges.AddRangeAsync(standardRate, evFriendlyRate);
            await _context.SaveChangesAsync();
        }

        // 3. Parqueadero / Zonas / Vehiculos / Grupos de Espacio / Puertas
        if (!await _context.Carparks.AnyAsync())
        {
            _logger.LogInformation("Seeding carpark, zones, vehicles, space groups and gates...");
            var firstCharge = await _context.Charges.OrderBy(x => x.Id).FirstAsync();
            var evRate = await _context.Charges.OrderBy(x => x.Id).Skip(1).FirstAsync();

            var carpark = new Carpark
            {
                Name = new MultiCodeName("CP", "Parqueadero Principal", "Sede Principal"),
                Address = new MultiName("Calle 72 #10-34, Bogota", "Calle 72 #10-34, Bogota"),
                CompanyName = new MultiName("Parqueaderos Colombia SAS", "Parqueaderos Colombia SAS"),
                ContactPerson = "Andres Martinez",
                PhoneNumber = "+57 601 744 1234",
                Email = "info@parqueaderocolombia.co",
                Description = "Parqueadero de demostracion con multiples zonas (principal, sotano, VIP)."
            };

            // Zones
            var mainZone = new Zone
            {
                Name = new MultiCodeName("MAIN", "Zona Principal", "Zona Principal"),
                IsMain = true,
                Description = "Primary entry / exit area.",
                HourlySets = new HourlySets(),
                MonthlySets = new MonthlySets(),
                Vehicles = [],
                Gates = [],
                SpaceGroups = []
            };
            var basementZone = new Zone
            {
                Name = new MultiCodeName("B1", "Sotano B1", "Sotano B1"),
                HourlySets = new HourlySets(),
                MonthlySets = new MonthlySets(),
                IsMain = false,
                Description = "General long stay parking.",
            };
            var vipZone = new Zone
            {
                Name = new MultiCodeName("VIP", "VIP Nivel 2", "VIP Nivel 2"),
                HourlySets = new HourlySets(),
                MonthlySets = new MonthlySets(),
                IsMain = false,
                Description = "Reserved spaces for premium members.",
            };

            carpark.Zones = [mainZone, basementZone, vipZone];

            // Vehicles (Hourly & Monthly types)
            mainZone.Vehicles =
            [
                new Vehicle { Name = "Carro Por Hora", ServiceCategoryId = ServiceCategories.Hourly,  VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 180, AllowEntryWhenFull = true,  Charge = firstCharge, Zone = mainZone },
                new Vehicle { Name = "Moto Por Hora",  ServiceCategoryId = ServiceCategories.Hourly,  VehicleTypeId = VehicleTypes.MotorCycle, Capacity = 40,  AllowEntryWhenFull = false, Charge = firstCharge, Zone = mainZone },
                new Vehicle { Name = "Carro Mensual",ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 120, AllowEntryWhenFull = false, Zone = mainZone },
                new Vehicle { Name = "Carro Electrico Por Hora", ServiceCategoryId = ServiceCategories.Hourly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 30, Charge = evRate, AllowEntryWhenFull = true, Zone = mainZone }
            ];
            basementZone.Vehicles =
            [
                new Vehicle { Name = "Sotano Por Hora", ServiceCategoryId = ServiceCategories.Hourly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 200, Charge = firstCharge, Zone = basementZone },
                new Vehicle { Name = "Sotano Mensual", ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 150, Zone = basementZone }
            ];
            vipZone.Vehicles =
            [
                new Vehicle { Name = "VIP Mensual", ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 40, Zone = vipZone }
            ];

            // Space Groups (Monthly Space Groupings)
            basementZone.SpaceGroups =
            [
                new SpaceGroup { Name = "B1-Oriente-Fijo", Capacity = 30, Description = "Reserved monthly spaces east wing." },
                new SpaceGroup { Name = "B1-Occidente-Flotante", Capacity = 70, Description = "Floating monthly allocation west wing." }
            ];
            vipZone.SpaceGroups =
            [
                new SpaceGroup { Name = "VIP-Oro", Capacity = 20, Description = "Gold tier reserved spaces." },
                new SpaceGroup { Name = "VIP-Platino", Capacity = 10, Description = "Platinum exclusive spaces." }
            ];

            // Gates (Entry / Exit)
            mainZone.Gates =
            [
                new Gate { Name = "Entrada Principal A", GateType = GateType.Entry, LaneNo = 1, Description = "Primary vehicle entry." },
                new Gate { Name = "Salida Principal A",  GateType = GateType.Exit,  LaneNo = 1, Description = "Primary vehicle exit." },
                new Gate { Name = "Entrada Principal B", GateType = GateType.Entry, LaneNo = 2, Description = "Secondary entry lane." }
            ];
            basementZone.Gates = [ new Gate { Name = "Rampa B1", GateType = GateType.EntryExit, LaneNo = 3, Description = "Ramp connecting to main." } ];
            vipZone.Gates = [ new Gate { Name = "Puerta VIP", GateType = GateType.EntryExit, LaneNo = 4, Description = "Restricted access gate." } ];
            carpark.TenantId = tenant.Id;
            await _context.Carparks.AddAsync(carpark);
            await _context.SaveChangesAsync();
        }

        // 4. Members (Monthly)
        if (!await _context.Members.AnyAsync())
        {
            _logger.LogInformation("Seeding members, member vehicles and rentals...");
            var now = DateTime.UtcNow.Date;
            var oneYearLater = now.AddYears(1).AddDays(-1);

            // Obtener SpaceGroup y Vehiculos Mensuales disponibles
            var monthlyVehicles = await _context.Vehicles.Where(v => v.ServiceCategoryId == ServiceCategories.Monthly).ToListAsync();
            var firstGroup = await _context.SpaceGroups.OrderBy(x => x.Id).FirstOrDefaultAsync();
            var vipPlatinum = await _context.SpaceGroups.FirstOrDefaultAsync(x => x.Name == "VIP-Platinum");

            var member1 = new Member
            {
                TenantId = tenant.Id,
                LicensePlate = "AB1234",
                CardId = "CARD-0001",
                StartDate = now,
                ExpiryDate = oneYearLater,
                SpaceGroupId = firstGroup?.Id,
                SpaceType = SpaceTypes.Regular,
                SpaceNo = "B1-E-021",
                Name = "Carlos Perez",
                PhoneNumber = "+57 311 111 1111",
                MobileNumber = "+57 311 111 1111",
                Email = "carlos.perez@email.co",
                Address = "Carrera 7 #72-10, Apto 501, Bogota",
                Notes = "Prefers east wing spot.",
                MemberVehicles = monthlyVehicles.Take(1).Select(v => new MemberVehicle { VehicleId = v.Id }).ToList()
            };
            var member2 = new Member
            {
                TenantId = tenant.Id,
                LicensePlate = "EV8888",
                CardId = "CARD-0002",
                StartDate = now.AddDays(-10),
                ExpiryDate = oneYearLater,
                SpaceGroupId = vipPlatinum?.Id,
                SpaceType = SpaceTypes.Reserved,
                SpaceNo = "VIP-P-05",
                Name = "Maria Gomez",
                PhoneNumber = "+57 322 222 2222",
                MobileNumber = "+57 322 222 2222",
                Email = "maria.gomez@email.co",
                Address = "Calle 80 #8-45, Apto 302, Bogota",
                Notes = "EV driver, VIP platinum.",
                MemberVehicles = monthlyVehicles.Skip(1).Take(1).Select(v => new MemberVehicle { VehicleId = v.Id }).ToList()
            };
            var member3 = new Member
            {
                TenantId = tenant.Id,
                LicensePlate = "MC3456",
                CardId = "CARD-0003",
                StartDate = now.AddMonths(-2),
                ExpiryDate = oneYearLater,
                SpaceGroupId = firstGroup?.Id,
                SpaceType = SpaceTypes.Floating,
                SpaceNo = "",
                Name = "Juan Rodriguez",
                PhoneNumber = "+57 333 333 3333",
                MobileNumber = "+57 333 333 3333",
                Email = "juan.rodriguez@email.co",
                Address = "Avenida Caracas #57-22, Bogota",
                Notes = "Floating allocation.",
                MemberVehicles = monthlyVehicles.Take(1).Select(v => new MemberVehicle { VehicleId = v.Id }).ToList()
            };

            await _context.Members.AddRangeAsync(member1, member2, member3);
            await _context.SaveChangesAsync();

            // Rentals
            var rentals = new List<MemberRental>
            {
                new MemberRental
                {
                    MemberId = member1.Id,
                    LicensePlate = member1.LicensePlate,
                    CardId = member1.CardId,
                    StartDate = member1.StartDate,
                    ExpiryDate = member1.StartDate.AddMonths(1).AddDays(-1),
                    RentalFee = 2500m,
                    Deposit = 150m,
                    AmountDue = 2650m,
                    AmountPaid = 2650m,
                    PaymentMethodId = PaymentMethods.Cash,
                    Notes = "Initial monthly payment."
                },
                new MemberRental
                {
                    MemberId = member2.Id,
                    LicensePlate = member2.LicensePlate,
                    CardId = member2.CardId,
                    StartDate = member2.StartDate,
                    ExpiryDate = member2.StartDate.AddMonths(1).AddDays(-1),
                    RentalFee = 4200m,
                    Deposit = 150m,
                    AmountDue = 4350m,
                    AmountPaid = 4350m,
                    PaymentMethodId = PaymentMethods.CreditCard,
                    Notes = "VIP platinum monthly fee."
                },
                new MemberRental
                {
                    MemberId = member3.Id,
                    LicensePlate = member3.LicensePlate,
                    CardId = member3.CardId,
                    StartDate = member3.StartDate,
                    ExpiryDate = member3.StartDate.AddMonths(1).AddDays(-1),
                    RentalFee = 2300m,
                    Deposit = 150m,
                    AmountDue = 2450m,
                    AmountPaid = 2450m,
                    PaymentMethodId = PaymentMethods.Cash,
                    Notes = "Floating monthly fee."
                }
            };
            await _context.MemberRentals.AddRangeAsync(rentals);
            await _context.SaveChangesAsync();
        }

        // 5. Dias Festivos (ejemplo, no duplicar si ya existen)
        if (!await _context.Holidays.AnyAsync())
        {
            _logger.LogInformation("Seeding holidays...");
            var year = DateTime.UtcNow.Year;
            var holidays = new List<Holiday>
            {
                new Holiday { Date = new DateTime(year, 1, 1),  Name_En = "New Year's Day",               Name_Tc = "Ano Nuevo" },
                new Holiday { Date = new DateTime(year, 4, 17), Name_En = "Maundy Thursday",             Name_Tc = "Jueves Santo" },
                new Holiday { Date = new DateTime(year, 4, 18), Name_En = "Good Friday",                 Name_Tc = "Viernes Santo" },
                new Holiday { Date = new DateTime(year, 5, 1),  Name_En = "Labour Day",                   Name_Tc = "Dia del Trabajo" },
                new Holiday { Date = new DateTime(year, 7, 20), Name_En = "Independence Day",             Name_Tc = "Dia de la Independencia" },
                new Holiday { Date = new DateTime(year, 8, 7),  Name_En = "Battle of Boyaca",             Name_Tc = "Batalla de Boyaca" },
                new Holiday { Date = new DateTime(year, 10, 13),Name_En = "Day of the Race",              Name_Tc = "Dia de la Raza" },
                new Holiday { Date = new DateTime(year, 11, 3), Name_En = "All Saints' Day",              Name_Tc = "Todos los Santos" },
                new Holiday { Date = new DateTime(year, 12, 25),Name_En = "Christmas Day",                Name_Tc = "Navidad" },
                new Holiday { Date = new DateTime(year, 12, 31),Name_En = "New Year's Eve",               Name_Tc = "Fin de Ano" },
            };
            await _context.Holidays.AddRangeAsync(holidays);
            await _context.SaveChangesAsync();
        }

        await UpsertBaseParkingRatesAsync();
        await UpsertBaseMonthlyRatesAsync();
        await UpsertBaseWashAdditionalsAsync();
        await UpsertBaseWashServicePricesAsync();
        await UpsertCompanyInformationAsync();
    }

    private async Task UpsertCompanyInformationAsync()
    {
        if (await _context.CompanyInformation.AnyAsync(x => x.IsActive))
            return;

        _context.CompanyInformation.Add(new CompanyInformation
        {
            DisplayName = "POLIEDRO SOFTWARE",
            TradeName = "POLIEDRO PARKING",
            TaxId = "NIT: 900.123.456-7",
            Address = "Bogota D.C.",
            Phone = "+57 (601) 123 4567",
            FooterText = "Soluciones de Parqueo",
            IsActive = true
        });

        await _context.SaveChangesAsync();
    }

    private async Task UpsertBaseParkingRatesAsync()
    {
        _logger.LogInformation("Upserting base parking rates...");
        var existingRates = await _context.ParkingRates.ToListAsync();

        foreach (var rate in ParkingRateDefaults.Rates)
        {
            var baseName = $"Base - {rate.Name}";
            var existingBase = existingRates.FirstOrDefault(x =>
                ParkingRateDefaults.IsMarkedBase(x.Description) &&
                x.VehicleType == rate.VehicleType);
            var shouldCreateVisible = existingBase is null ||
                                      existingBase.Name.Equals(rate.Name, StringComparison.OrdinalIgnoreCase);

            if (existingBase is null)
            {
                _context.ParkingRates.Add(new ParkingRate
                {
                    Name = baseName,
                    VehicleType = rate.VehicleType,
                    HourlyRate = rate.HourlyRate,
                    DayRate = rate.DayRate,
                    NightRate = rate.NightRate,
                    IsActive = true,
                    Description = ParkingRateDefaults.MarkDescription(rate.Description)
                });
            }
            else
            {
                existingBase.Name = baseName;
                existingBase.VehicleType = rate.VehicleType;
                existingBase.HourlyRate = rate.HourlyRate;
                existingBase.DayRate = rate.DayRate;
                existingBase.NightRate = rate.NightRate;
                existingBase.IsActive = true;
                existingBase.Description = ParkingRateDefaults.MarkDescription(rate.Description);
            }

            var visibleExists = existingRates.Any(x =>
                !ParkingRateDefaults.IsMarkedBase(x.Description) &&
                x.VehicleType == rate.VehicleType);

            if (!visibleExists && shouldCreateVisible)
                _context.ParkingRates.Add(new ParkingRate
                {
                    Name = rate.Name,
                    VehicleType = rate.VehicleType,
                    HourlyRate = rate.HourlyRate,
                    DayRate = rate.DayRate,
                    NightRate = rate.NightRate,
                    IsActive = true,
                    Description = rate.Description
                });
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpsertBaseMonthlyRatesAsync()
    {
        _logger.LogInformation("Upserting base monthly rates...");
        var existingRates = await _context.MonthlyRates.ToListAsync();

        foreach (var rate in MonthlyRateDefaults.Rates)
        {
            var baseName = $"Base - {rate.Name}";
            var existingBase = existingRates.FirstOrDefault(x =>
                MonthlyRateDefaults.IsMarkedBase(x.Description) &&
                x.VehicleType == rate.VehicleType);
            var shouldCreateVisible = existingBase is null ||
                                      existingBase.Name.Equals(rate.Name, StringComparison.OrdinalIgnoreCase);

            if (existingBase is null)
            {
                _context.MonthlyRates.Add(new MonthlyRate
                {
                    Name = baseName,
                    VehicleType = rate.VehicleType,
                    MonthlyFee = rate.MonthlyFee,
                    Deposit = rate.Deposit,
                    IsActive = true,
                    Description = MonthlyRateDefaults.MarkDescription(rate.Description)
                });
            }
            else
            {
                existingBase.Name = baseName;
                existingBase.VehicleType = rate.VehicleType;
                existingBase.MonthlyFee = rate.MonthlyFee;
                existingBase.Deposit = rate.Deposit;
                existingBase.IsActive = true;
                existingBase.Description = MonthlyRateDefaults.MarkDescription(rate.Description);
            }

            var visibleExists = existingRates.Any(x =>
                !MonthlyRateDefaults.IsMarkedBase(x.Description) &&
                x.VehicleType == rate.VehicleType &&
                x.Name.Equals(rate.Name, StringComparison.OrdinalIgnoreCase));

            if (!visibleExists && shouldCreateVisible)
                _context.MonthlyRates.Add(new MonthlyRate
                {
                    Name = rate.Name,
                    VehicleType = rate.VehicleType,
                    MonthlyFee = rate.MonthlyFee,
                    Deposit = rate.Deposit,
                    IsActive = true,
                    Description = rate.Description
                });
        }

        await _context.SaveChangesAsync();
    }

    private async Task UpsertBaseWashAdditionalsAsync()
    {
        _logger.LogInformation("Upserting base wash additionals...");
        var existingAdditionals = await _context.WashAdditionals.ToListAsync();

        foreach (var additional in WashCatalogDefaults.Additionals)
        {
            var baseName = $"Base - {additional.Name}";
            var existingBase = existingAdditionals.FirstOrDefault(x =>
                WashCatalogDefaults.IsMarkedBase(x.Description) &&
                (x.Name.Equals(additional.Name, StringComparison.OrdinalIgnoreCase) ||
                 x.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)));
            var shouldCreateVisible = existingBase is null ||
                                      existingBase.Name.Equals(additional.Name, StringComparison.OrdinalIgnoreCase);

            if (existingBase is null)
            {
                _context.WashAdditionals.Add(new WashAdditional
                {
                    Name = baseName,
                    Price = additional.Price,
                    IsActive = true,
                    Description = WashCatalogDefaults.MarkDescription(additional.Description)
                });
            }
            else
            {
                existingBase.Name = baseName;
                existingBase.Price = additional.Price;
                existingBase.IsActive = true;
                existingBase.Description = WashCatalogDefaults.MarkDescription(additional.Description);
            }

            var visibleExists = existingAdditionals.Any(x =>
                !WashCatalogDefaults.IsMarkedBase(x.Description) &&
                x.Name.Equals(additional.Name, StringComparison.OrdinalIgnoreCase));

            if (!visibleExists && shouldCreateVisible)
                _context.WashAdditionals.Add(new WashAdditional
                {
                    Name = additional.Name,
                    Price = additional.Price,
                    IsActive = true,
                    Description = additional.Description
                });
        }

        await _context.SaveChangesAsync();
        await AlignBaseWashAdditionalsAsync();
    }

    private async Task UpsertBaseWashServicePricesAsync()
    {
        _logger.LogInformation("Upserting base wash service prices...");
        var existingPrices = await _context.WashServicePrices.ToListAsync();

        foreach (var price in WashCatalogDefaults.ServicePrices)
        {
            var baseName = $"Base - {price.Name}";
            var existingBase = existingPrices.FirstOrDefault(x =>
                WashCatalogDefaults.IsMarkedBase(x.Description) &&
                x.ServiceType == price.ServiceType &&
                x.VehicleType == price.VehicleType);
            var shouldCreateVisible = existingBase is null ||
                                      existingBase.Name.Equals(price.Name, StringComparison.OrdinalIgnoreCase);

            if (existingBase is null)
            {
                _context.WashServicePrices.Add(new WashServicePrice
                {
                    Name = baseName,
                    ServiceType = price.ServiceType,
                    VehicleType = price.VehicleType,
                    BasePrice = price.BasePrice,
                    IsActive = true,
                    Description = WashCatalogDefaults.MarkDescription(price.Description)
                });
            }
            else
            {
                existingBase.Name = baseName;
                existingBase.ServiceType = price.ServiceType;
                existingBase.VehicleType = price.VehicleType;
                existingBase.BasePrice = price.BasePrice;
                existingBase.IsActive = true;
                existingBase.Description = WashCatalogDefaults.MarkDescription(price.Description);
            }

            var visibleExists = existingPrices.Any(x =>
                !WashCatalogDefaults.IsMarkedBase(x.Description) &&
                x.ServiceType == price.ServiceType &&
                x.VehicleType == price.VehicleType);

            if (!visibleExists && shouldCreateVisible)
                _context.WashServicePrices.Add(new WashServicePrice
                {
                    Name = price.Name,
                    ServiceType = price.ServiceType,
                    VehicleType = price.VehicleType,
                    BasePrice = price.BasePrice,
                    IsActive = true,
                    Description = price.Description
                });
        }

        await _context.SaveChangesAsync();
        await AlignBaseWashServicePricesAsync();
    }

    private async Task AlignBaseWashAdditionalsAsync()
    {
        foreach (var additional in WashCatalogDefaults.Additionals)
        {
            var existingUsages = await _context.CarWashAdditionals
                .Where(x => x.AdditionalName.ToUpper() == additional.Name.ToUpper() &&
                            x.CarWash != null &&
                            !x.CarWash.IsPaid)
                .ToListAsync();

            foreach (var usage in existingUsages)
            {
                usage.AdditionalName = additional.Name;
                usage.Price = additional.Price;
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task AlignBaseWashServicePricesAsync()
    {
        foreach (var price in WashCatalogDefaults.ServicePrices)
        {
            var existingWashes = await _context.CarWashes
                .Include(x => x.Additionals)
                .Where(x => x.WashServiceType == price.ServiceType &&
                            x.VehicleType == price.VehicleType &&
                            !x.IsPaid)
                .ToListAsync();

            foreach (var wash in existingWashes)
            {
                var additionalsTotal = wash.Additionals.Sum(x => x.Price);
                wash.Price = price.BasePrice + additionalsTotal + wash.WeekendSurcharge;
            }
        }

        await _context.SaveChangesAsync();
    }
}
