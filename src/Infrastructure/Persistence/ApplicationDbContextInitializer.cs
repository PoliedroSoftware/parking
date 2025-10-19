using System;
using System.Reflection;
using CleanArchitecture.Blazor.Application.Common.Constants;
using CleanArchitecture.Blazor.Application.Common.Security;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;
using CleanArchitecture.Blazor.Domain.Common;

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

        if (await _roleManager.RoleExistsAsync(adminRoleName)) return;

        _logger.LogInformation("Seeding roles...");
        var administratorRole = new ApplicationRole(adminRoleName)
        {
            Description = "Admin Group",
            CreatedAt= DateTime.UtcNow,
        };
        var userRole = new ApplicationRole(userRoleName)
        {
            Description = "Basic Group",
            CreatedAt = DateTime.UtcNow,
        };

        await _roleManager.CreateAsync(administratorRole);
        await _roleManager.CreateAsync(userRole);

        var permissions = GetAllPermissions();

        foreach (var permission in permissions)
        {
            var claim = new Claim(ApplicationClaimTypes.Permission, permission);
            await _roleManager.AddClaimAsync(administratorRole, claim);

            if (permission.StartsWith("Permissions.Products"))
            {
                await _roleManager.AddClaimAsync(userRole, claim);
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
        // 1. 基础字典数据 (仅首次种子)
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

        // 2. 收費方案 (Charges)
        if (!await _context.Charges.AnyAsync())
        {
            _logger.LogInformation("Seeding charges (rate plans)...");
            var standardRate = new Charge
            {
                Name = "Standard Hourly Rate",
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
                        new ChargePeriod { StartTime = "07:30", EndTime = "09:30" }, // Morning Peak
                        new ChargePeriod { StartTime = "17:00", EndTime = "19:30" }, // Evening Peak
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 18, PriceHoliday = 20 },
                        new ChargeItem { Duration = 60, PriceWeekday = 32, PriceHoliday = 36 },
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "08:00", EndTime = "20:00" } ],
                        CeilingAmountWeekday = 120,
                        CeilingAmountHoliday = 140
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "20:00", EndTime = "08:00" } ],
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
                        new ChargePeriod { StartTime = "18:00", EndTime = "23:00" }
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 10, PriceHoliday = 12 },
                        new ChargeItem { Duration = 60, PriceWeekday = 17, PriceHoliday = 21 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [new ChargePeriod { StartTime = "08:00", EndTime = "20:00" }],
                        CeilingAmountWeekday = 115,
                        CeilingAmountHoliday = 135
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [new ChargePeriod { StartTime = "18:30", EndTime = "07:30" }],
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
                Name = "EV Friendly Rate",
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
                        new ChargePeriod { StartTime = "18:30", EndTime = "22:30" } // Evening promo period
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 9, PriceHoliday = 11 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "08:00", EndTime = "20:00" } ],
                        CeilingAmountWeekday = 110,
                        CeilingAmountHoliday = 130
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "19:00", EndTime = "07:00" } ],
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
                        new ChargePeriod { StartTime = "18:00", EndTime = "23:00" }
                    ],
                    SpecialCharges =
                    [
                        new ChargeItem { Duration = 30, PriceWeekday = 10, PriceHoliday = 12 },
                        new ChargeItem { Duration = 60, PriceWeekday = 17, PriceHoliday = 21 }
                    ],
                    DayPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "08:00", EndTime = "20:00" } ],
                        CeilingAmountWeekday = 115,
                        CeilingAmountHoliday = 135
                    },
                    NightPark = new ReducedItem
                    {
                        Period = [ new ChargePeriod { StartTime = "18:30", EndTime = "07:30" } ],
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

        // 3. 停車場 / 區域 / 車類 / 車位組 / 閘機
        if (!await _context.Carparks.AnyAsync())
        {
            _logger.LogInformation("Seeding carpark, zones, vehicles, space groups and gates...");
            var firstCharge = await _context.Charges.OrderBy(x => x.Id).FirstAsync();
            var evRate = await _context.Charges.OrderBy(x => x.Id).Skip(1).FirstAsync();

            var carpark = new Carpark
            {
                Name = new MultiCodeName("CP", "Central Plaza Carpark", "中環廣場停車場"),
                Address = new MultiName("1 Finance Street, Central", "中環金融街1號"),
                CompanyName = new MultiName("Central Plaza Management Ltd", "中環廣場物業管理有限公司"),
                ContactPerson = "Alex Chan",
                PhoneNumber = "+852 2888 1234",
                Email = "info@centralplaza.example",
                Description = "A demonstration carpark with multi zones (main, basement, VIP)."
            };

            // Zones
            var mainZone = new Zone
            {
                Name = new MultiCodeName("MAIN", "Main Concourse", "主層出入口"),
                IsMain = true,
                Description = "Primary entry / exit area.",
                Vehicles = [],
                Gates = [],
                SpaceGroups = []
            };
            var basementZone = new Zone
            {
                Name = new MultiCodeName("B1", "Basement B1", "地庫一層"),
                IsMain = false,
                Description = "General long stay parking.",
            };
            var vipZone = new Zone
            {
                Name = new MultiCodeName("VIP", "VIP Level 2", "貴賓二層"),
                IsMain = false,
                Description = "Reserved spaces for premium members.",
            };

            carpark.Zones = [mainZone, basementZone, vipZone];

            // Vehicles (Hourly & Monthly types)
            mainZone.Vehicles =
            [
                new Vehicle { Name = "Private Car Hourly", ServiceCategoryId = ServiceCategories.Hourly,  VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 180, AllowEntryWhenFull = true,  Charge = firstCharge, Zone = mainZone },
                new Vehicle { Name = "Motorcycle Hourly",  ServiceCategoryId = ServiceCategories.Hourly,  VehicleTypeId = VehicleTypes.MotorCycle, Capacity = 40,  AllowEntryWhenFull = false, Charge = firstCharge, Zone = mainZone },
                new Vehicle { Name = "Private Car Monthly",ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 120, AllowEntryWhenFull = false, Zone = mainZone },
                new Vehicle { Name = "EV Private Car Hourly", ServiceCategoryId = ServiceCategories.Hourly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 30, Charge = evRate, AllowEntryWhenFull = true, Zone = mainZone }
            ];
            basementZone.Vehicles =
            [
                new Vehicle { Name = "Basement Hourly", ServiceCategoryId = ServiceCategories.Hourly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 200, Charge = firstCharge, Zone = basementZone },
                new Vehicle { Name = "Basement Monthly", ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 150, Zone = basementZone }
            ];
            vipZone.Vehicles =
            [
                new Vehicle { Name = "VIP Monthly", ServiceCategoryId = ServiceCategories.Monthly, VehicleTypeId = VehicleTypes.PrivateCar, Capacity = 40, Zone = vipZone }
            ];

            // Space Groups (Monthly Space Groupings)
            basementZone.SpaceGroups =
            [
                new SpaceGroup { Name = "B1-East-Reserved", Capacity = 30, Description = "Reserved monthly spaces east wing." },
                new SpaceGroup { Name = "B1-West-Floating", Capacity = 70, Description = "Floating monthly allocation west wing." }
            ];
            vipZone.SpaceGroups =
            [
                new SpaceGroup { Name = "VIP-Gold", Capacity = 20, Description = "Gold tier reserved spaces." },
                new SpaceGroup { Name = "VIP-Platinum", Capacity = 10, Description = "Platinum exclusive spaces." }
            ];

            // Gates (Entry / Exit)
            mainZone.Gates =
            [
                new Gate { Name = "Main Entry A", GateType = GateType.Entry, LaneNo = 1, Description = "Primary vehicle entry." },
                new Gate { Name = "Main Exit A",  GateType = GateType.Exit,  LaneNo = 1, Description = "Primary vehicle exit." },
                new Gate { Name = "Main Entry B", GateType = GateType.Entry, LaneNo = 2, Description = "Secondary entry lane." }
            ];
            basementZone.Gates = [ new Gate { Name = "B1 Ramp", GateType = GateType.EntryExit, LaneNo = 3, Description = "Ramp connecting to main." } ];
            vipZone.Gates = [ new Gate { Name = "VIP Gate", GateType = GateType.EntryExit, LaneNo = 4, Description = "Restricted access gate." } ];

            await _context.Carparks.AddAsync(carpark);
            await _context.SaveChangesAsync();
        }

        // 4. Members (Monthly)
        if (!await _context.Members.AnyAsync())
        {
            _logger.LogInformation("Seeding members, member vehicles and rentals...");
            var now = DateTime.UtcNow.Date;
            var oneYearLater = now.AddYears(1).AddDays(-1);

            // 获取可用 SpaceGroup & 月租 Vehicle
            var monthlyVehicles = await _context.Vehicles.Where(v => v.ServiceCategoryId == ServiceCategories.Monthly).ToListAsync();
            var firstGroup = await _context.SpaceGroups.OrderBy(x => x.Id).FirstOrDefaultAsync();
            var vipPlatinum = await _context.SpaceGroups.FirstOrDefaultAsync(x => x.Name == "VIP-Platinum");

            var member1 = new Member
            {
                LicensePlate = "AB1234",
                CardId = "CARD-0001",
                StartDate = now,
                ExpiryDate = oneYearLater,
                SpaceGroupId = firstGroup?.Id,
                SpaceType = SpaceTypes.Regular,
                SpaceNo = "B1-E-021",
                Name = "Jason Lee",
                PhoneNumber = "+852 6111 1111",
                MobileNumber = "+852 6111 1111",
                Email = "jason.lee@example",
                Address = "Flat A, 10/F, Central Plaza",
                Notes = "Prefers east wing spot.",
                MemberVehicles = monthlyVehicles.Take(1).Select(v => new MemberVehicle { VehicleId = v.Id }).ToList()
            };
            var member2 = new Member
            {
                LicensePlate = "EV8888",
                CardId = "CARD-0002",
                StartDate = now.AddDays(-10),
                ExpiryDate = oneYearLater,
                SpaceGroupId = vipPlatinum?.Id,
                SpaceType = SpaceTypes.Reserved,
                SpaceNo = "VIP-P-05",
                Name = "Emily Wong",
                PhoneNumber = "+852 6222 2222",
                MobileNumber = "+852 6222 2222",
                Email = "emily.wong@example",
                Address = "Flat B, 8/F, Central Plaza",
                Notes = "EV driver, VIP platinum.",
                MemberVehicles = monthlyVehicles.Skip(1).Take(1).Select(v => new MemberVehicle { VehicleId = v.Id }).ToList()
            };
            var member3 = new Member
            {
                LicensePlate = "MC3456",
                CardId = "CARD-0003",
                StartDate = now.AddMonths(-2),
                ExpiryDate = oneYearLater,
                SpaceGroupId = firstGroup?.Id,
                SpaceType = SpaceTypes.Floating,
                SpaceNo = "",
                Name = "Ricky Ho",
                PhoneNumber = "+852 6333 3333",
                MobileNumber = "+852 6333 3333",
                Email = "ricky.ho@example",
                Address = "Flat C, 12/F, Central Plaza",
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
                    PaymentMethodId = PaymentMethods.Octopus,
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
                    PaymentMethodId = PaymentMethods.Octopus,
                    Notes = "Floating monthly fee."
                }
            };
            await _context.MemberRentals.AddRangeAsync(rentals);
            await _context.SaveChangesAsync();
        }

        // 5. Holidays (示例假期，如已存在不重复添加)
        if (!await _context.Holidays.AnyAsync())
        {
            _logger.LogInformation("Seeding holidays...");
            var year = DateTime.UtcNow.Year;
            var holidays = new List<Holiday>
            {
                new Holiday { Date = new DateTime(year, 1, 1),  Name_En = "New Year's Day",               Name_Tc = "元旦" },
                new Holiday { Date = new DateTime(year, 2, 1),  Name_En = "Lunar New Year (Day 1)",       Name_Tc = "農曆新年初一" },
                new Holiday { Date = new DateTime(year, 2, 2),  Name_En = "Lunar New Year (Day 2)",       Name_Tc = "農曆新年初二" },
                new Holiday { Date = new DateTime(year, 4, 5),  Name_En = "Ching Ming Festival",          Name_Tc = "清明節" },
                new Holiday { Date = new DateTime(year, 5, 1),  Name_En = "Labour Day",                   Name_Tc = "勞動節" },
                new Holiday { Date = new DateTime(year, 7, 1),  Name_En = "HKSAR Establishment Day",      Name_Tc = "香港特區成立紀念日" },
                new Holiday { Date = new DateTime(year, 10, 1), Name_En = "National Day",                Name_Tc = "國慶節" },
                new Holiday { Date = new DateTime(year, 12, 25),Name_En = "Christmas Day",               Name_Tc = "聖誕節" },
                new Holiday { Date = new DateTime(year, 12, 26),Name_En = "Boxing Day",                  Name_Tc = "節禮日" },
            };
            await _context.Holidays.AddRangeAsync(holidays);
            await _context.SaveChangesAsync();
        }
    }
}
