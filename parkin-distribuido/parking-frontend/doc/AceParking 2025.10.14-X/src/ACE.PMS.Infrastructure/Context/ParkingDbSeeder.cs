using ACE.PMS.Domain.Entities;
using ACE.PMS.Infrastructure.Conversions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ACE.PMS.Infrastructure.Context;

public class ParkingDbSeeder
{
    private readonly ParkingDbContext _context;

    public ParkingDbSeeder(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task SeedDatabaseAsync()
    {

        //var zone = await _context.Zones.FindAsync(3) ?? throw new ArgumentNullException(nameof(Zone));

        try
        {
            await SeedCarparksAsync();
            await SeedZonesAsync();
            await SeedChargesAsync();
            await SeedVehiclesAsync();

            await SeedGatesAsync();

            await SeedSpaceGroupsAsync();
            await SeedMembersAsync();

            await SeedHolidaysAsync();                        

            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception)
        {
            throw;
        }
    }

    #region Seed Carparks
    private async Task SeedCarparksAsync()
    {
        if (await _context.Carparks.AnyAsync()) return;
        Carpark[] carparks =
        [
            new Carpark()
            {
                AppKey = Guid.CreateVersion7().ToString(),
                Name = new MultiCodeName("ABC", "Car Park ABC", "ABC停車場"),
                Address = new MultiName("Kowloon,Hong Kong", "香港九龍"),
                CompanyName = new MultiName("ABC Parking Management Limited", "ABC停車場管理有限公司"),
            },
        ];
        await _context.Carparks.AddRangeAsync(carparks);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Seed Zones
    private async Task SeedZonesAsync()
    {
        if (await _context.Zones.AnyAsync()) return;

        Zone[] zones =
        [
            new Zone()
            {
                Name = new MultiCodeName("Main", "Car Park Main", "大閘停車場"),
                CarparkId = 1,
                Carpark=await _context.Carparks.FindAsync(1)?? throw new ArgumentNullException(nameof(Carpark)),
                HourlySets = new()
                {
                    Capacity = 150,
                },
                Description = "大閘停車場，Main",
            },
            new Zone()
            {
                Name = new MultiCodeName("B1", "Car Park B1", "B1停車場"),
                CarparkId = 1,
                Carpark=await _context.Carparks.FindAsync(1)?? throw new ArgumentNullException(nameof(Carpark)),
                HourlySets = new()
                {
                    Capacity = 20,
                },
                Description = "B1停車場，貨車卸貨場",
            },
            new Zone()
            {
                Name = new MultiCodeName("1F", "Car Park 1/F", "1/F停車場"),
                CarparkId = 1,
                Carpark=await _context.Carparks.FindAsync(1)?? throw new ArgumentNullException(nameof(Carpark)),
                HourlySets = new()
                {
                    Capacity = 130,
                },
                Description = "1/F停車場，私家車住戶場",
            },
        ];
        await _context.Zones.AddRangeAsync(zones);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Seed Charges
    private async Task SeedChargesAsync()
    {
        if (await _context.Charges.AnyAsync()) return;

        List<Charge> charges = [];

        // 1. 時租-私家車 Hourly-PrivateCar
        string contentJson = new RateContent()
        {
            NormalCharges = [new ChargeItem(60, 17, 17)],  //正常收費 （每小時或不足一小時）
            SpecialPeriod = ["23:00", "06:59"],            //特別時段：晚上11時至翌晨7時
            SpecialCharges = [new ChargeItem(60, 14, 14)], //特別時段收費
            DayPark = new ReducedItem(["08:00", "19:59"], 95, 95),   //日泊優惠 時段,最高收費
            NightPark = new ReducedItem(["20:00", "07:59"], 80, 80), //夜泊優惠 時段,最高收費
            Max24Park = new MaxReducedItem(true, 130, 130), //24小时泊最高收費 
        }.ToJsonString(camelCase: true);

        charges.Add(new Charge(1, "時租-私家車-收費表", new DateTime(2025, 9, 1), contentJson, contentJson)
        {
            Description = "Standard parking rates for Private cars (Car/Vans) vehicles."
        });


        // 2. 時租-電單車 Hourly-MotorCycle 
        contentJson = new RateContent()
        {
            NormalCharges = [new ChargeItem(60, 6, 6)], //正常收費 （每小時或不足一小時）
        }.ToJsonString(camelCase: true);

        charges.Add(new Charge(2, "時租-電單車-收費表", new DateTime(2025, 9, 1), contentJson, contentJson)
        {
            Description = "Standard parking rates for MotorCycles vehicles."
        });

        // 3. 時租-貨車 Hourly-LightGoods
        contentJson = new RateContent()
        {
            NormalCharges =
            [
                new ChargeItem(60, 24, 24), //時段1 （每小時或不足一小時）
                new ChargeItem(60, 24, 24), //時段2 （每小時或不足一小時）
                new ChargeItem(60, 48, 48), //其後  （每小時或不足一小時）
            ],
        }.ToJsonString(camelCase: true);

        charges.Add(new Charge(3, "時租-貨車-收費表", new DateTime(2025, 9, 1), contentJson, contentJson)
        {
            Description = "Standard parking rates for LightGoods (Lorry) vehicles."
        });

        // 4. 月租-夜泊 Monthly-PartTime
        contentJson = new RateContent()
        {
            NormalCharges = [new ChargeItem(60, 17, 17)],  //正常收費 （每小時或不足一小時）
            NightPark = new ReducedItem(["18:00", "07:59"], 0, 0), //優惠時段->夜泊月租出租時間段，免費
        }.ToJsonString(camelCase: true);

        charges.Add(new Charge(4, "月租-夜泊-收費表", new DateTime(2025, 9, 1), contentJson, contentJson)
        {
            Description = $"部分時間泊車 (由下午6時至翌晨8時或在此期間之時段出租的泊車位){Environment.NewLine}" +
                    $"Part time rate (for space let between 6:00 pm and 8:00 am of the following day or part thereof)."
        });

        await _context.Charges.AddRangeAsync(charges);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Seed Vehicles
    private async Task SeedVehiclesAsync()
    {
        if (await _context.Vehicles.AnyAsync()) return;

        //主場 Capacity:150
        var z1 = await _context.Zones.FindAsync(1) ?? throw new ArgumentNullException(nameof(Zone));
        Vehicle[] vehicles =
        [
            new Vehicle("時租-私家車", ServiceCategories.Hourly,VehicleTypes.PrivateCar)
            {
                Zone=z1,
                Charge=await _context.Charges.FindAsync(1) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 80,
            },
            new Vehicle("時租-電單車",ServiceCategories.Hourly,VehicleTypes.MotorCycle)
            {
                Zone=z1,
                Charge=await _context.Charges.FindAsync(2) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 15,
                CanRecognizePlate = false,
            },
            new Vehicle("時租-貨車",ServiceCategories.Hourly,VehicleTypes.LightGoods)
            {
                Zone=z1,
                Charge=await _context.Charges.FindAsync(3) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 5,
            },

            new Vehicle("月租-私家車",  ServiceCategories.Monthly,VehicleTypes.PrivateCar)
            {
                Zone=z1,
                Capacity = 50,
                AllowEntryWhenFull = true,
            },
            new Vehicle("月租-電單車",  ServiceCategories.Monthly,VehicleTypes.MotorCycle)
            {
                Zone=z1,
                Capacity = 10,
                AllowEntryWhenFull = true,
                CanRecognizePlate = false,
            },
            new Vehicle("月租-貨車",  ServiceCategories.Monthly,VehicleTypes.LightGoods)
            {
                Zone=z1,
                Capacity = 15,
                AllowEntryWhenFull = true,
            },
        ];
        await _context.Vehicles.AddRangeAsync(vehicles);

        //貨車場 B1
        var z2 = await _context.Zones.FindAsync(2) ?? throw new ArgumentNullException(nameof(Zone));
        vehicles =
        [
            // Hourly Vehicles            
            new Vehicle("B1-時租-貨車",  ServiceCategories.Hourly,VehicleTypes.LightGoods)
            {
                Zone=z2,
                Charge = await _context.Charges.FindAsync(2) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 10,
            },
        ];
        await _context.Vehicles.AddRangeAsync(vehicles);

        //私家車場 1/F
        var z3 = await _context.Zones.FindAsync(3) ?? throw new ArgumentNullException(nameof(Zone));
        vehicles =
        [
            new Vehicle("1F-時租-私家車", ServiceCategories.Hourly,VehicleTypes.PrivateCar)
            {
                Zone=z3,
                Charge=await _context.Charges.FindAsync(1) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 100,
            },
            new Vehicle("1F-月租-私家車", ServiceCategories.Monthly,VehicleTypes.PrivateCar)
            {
                Zone=z3,
                Capacity = 100,
            },
            new Vehicle("1F-月租夜泊-私家車",  ServiceCategories.Monthly,VehicleTypes.PrivateCar)
            {
                Zone=z3,
                Charge=await _context.Charges.FindAsync(4) ?? throw new ArgumentNullException(nameof(Charge)),
                Capacity = 50,
            },
        ];
        await _context.Vehicles.AddRangeAsync(vehicles);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Seed Gates
    private async Task SeedGatesAsync()
    {
        if (await _context.Gates.AnyAsync()) return;

        int zoneId = 1; //主場
        List<Gate> gates =
        [
            new Gate("大閘-入口",zoneId ,GateType.Entry),
            new Gate("大閘-出口",zoneId ,GateType.Exit ),
        ];

        zoneId = 2; //B1
        gates.Add(new Gate("B1-入口", zoneId, GateType.Entry));
        gates.Add(new Gate("B1-出口", zoneId, GateType.Exit));

        zoneId = 3; //1/F
        gates.Add(new Gate("1F-入口", zoneId, GateType.Entry));
        gates.Add(new Gate("1F-出口", zoneId, GateType.Exit));

        await _context.Gates.AddRangeAsync(gates);
        await _context.SaveChangesAsync();
    }
    #endregion   

    #region Seed SpaceGroup
    private async Task SeedSpaceGroupsAsync()
    {
        if (await _context.SpaceGroups.AnyAsync()) return;
        SpaceGroup[] groups =
        [
            new SpaceGroup("Main-A001", 1)
            {
                Zone=await _context.Zones.FindAsync(1)?? throw new ArgumentNullException(nameof(Zone)),
                Description = "Main A車位組 1車位",
            },
            new SpaceGroup("Main-A002",2)
            {
                Zone=await _context.Zones.FindAsync(1)?? throw new ArgumentNullException(nameof(Zone)),
                Description = "Main B車位組 2車位",
            },
            new SpaceGroup("1F-F001", 1)
            {
                Zone=await _context.Zones.FindAsync(3)?? throw new ArgumentNullException(nameof(Zone)),
                Description = "1/F 陳大文車位組 1車位",
            },
            new SpaceGroup("1F-F003",3)
            {
                Zone=await _context.Zones.FindAsync(3)?? throw new ArgumentNullException(nameof(Zone)),
                Description = "1/F 李小龍車位組 3車位",
            },
        ];
        await _context.SpaceGroups.AddRangeAsync(groups);
        await _context.SaveChangesAsync();
    }
    #endregion

    #region Seed Holidays
    private async Task SeedHolidaysAsync()
    {
        if (await _context.Holidays.AnyAsync()) return;

        var rootPath = AppContext.BaseDirectory;
        var fullPath = Path.Combine(rootPath, "Setup", "Holidays2025.json");
        var jsonString = File.ReadAllText(fullPath);

        var items = JsonSerializer.Deserialize<Holiday[]>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        if (items is not null)
        {
            await _context.Holidays.AddRangeAsync(items);
        }
    }
    #endregion

    #region Seed Members
    private async Task SeedMembersAsync()
    {
        if (await _context.Members.AnyAsync()) return;
        Member[] members =
        [
            new Member("64129","20864129",DateTime.Today.AddDays(-30), DateTime.Today.AddDays(335))
            {
                Vehicle=await _context.Vehicles.FirstOrDefaultAsync(v => v.Name == "月租-私家車") ?? throw new ArgumentNullException(nameof(Vehicle)),
                SpaceGroup = await _context.SpaceGroups.FirstOrDefaultAsync(g => g.Name == "Main-A001") ?? throw new ArgumentNullException(nameof(SpaceGroup)),
                AllowedZoneIds = [1,3], //主場,1/F
                SpaceType= SpaceTypes.ReservedOpen,
                SpaceNo= "A001",
                Name="Chris Wong",
                PhoneNumber="88763423",
                MobileNumber = "90695678",
                Email = "chriswong@gmail.com",
                Address = "John Doe,Hong Kong,香港",
                Notes = "Chris Wong，會員編號A0001，私家車",
             },
             new Member("55869","20655869",DateTime.Today.AddDays(-10), DateTime.Today.AddDays(335))
            {
                Vehicle=await _context.Vehicles.FirstOrDefaultAsync(v => v.Name == "月租-電單車") ?? throw new ArgumentNullException(nameof(Vehicle)),
                //SpaceGroup = await _context.SpaceGroups.FirstOrDefaultAsync(g => g.Name == "1F-F001") ?? throw new ArgumentNullException(nameof(SpaceGroup)),
                AllowedZoneIds = [1], //主場,1/F
                SpaceType= SpaceTypes.Floating,
                SpaceNo= "",
                Name="Bruce Lee",
                PhoneNumber="88349786",
                MobileNumber = "90459844",
                Email = "",
                Address = "Hong Kong,香港",
                Notes = "Bluce Lee，會員編號A0002，電單車",
             },
            new Member("57923","20657923",DateTime.Today.AddDays(-30), DateTime.Today.AddDays(335))
            {
                Vehicle=await _context.Vehicles.FirstOrDefaultAsync(v => v.Name == "1F-月租-私家車") ?? throw new ArgumentNullException(nameof(Vehicle)),
                SpaceGroup = await _context.SpaceGroups.FirstOrDefaultAsync(g => g.Name == "1F-F001") ?? throw new ArgumentNullException(nameof(SpaceGroup)),
                AllowedZoneIds = [1,3], //主場,1/F
                SpaceType= SpaceTypes.Reserved,
                SpaceNo= "F001",
                Name="陳大文",
                PhoneNumber="91234567",
                MobileNumber = "91234567",
                Email = "",
                Address = "Hong Kong,香港",
                Notes = "陳大文，會員編號F0001，私家車",
             },
        ];
        await _context.Members.AddRangeAsync(members);
        await _context.SaveChangesAsync();
    }
    #endregion
        
}


public class HolidayItem
{
    public DateTime Date { get; set; } 
    public string Name_En { get; set; } = string.Empty;
    public string Name_Tc { get; set; } = string.Empty;
}