
namespace ACE.PMS.Infrastructure.Context;

public class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Carpark> Carparks => Set<Carpark>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Gate> Gates => Set<Gate>();
    public DbSet<Charge> Charges => Set<Charge>();        
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<SpaceGroup> SpaceGroups => Set<SpaceGroup>();

    public DbSet<Holiday> Holidays => Set<Holiday>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        //關閉級聯刪除
        //foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()).Where(fk=> fk.DeleteBehavior==DeleteBehavior.Cascade))            
        //{
        //     property.DeleteBehavior = DeleteBehavior.Restrict;
        //}


        foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(9,1)"); //1-9 5bytes 10-19 9bytes 20-28 13bytes 29-38 19bytes
        }

        foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("datetime2(2)");
        }

        foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.Name is "CreatedBy" or "LastModifiedBy"))
        {
            property.SetColumnType("nvarchar(64)");

            if (property.Name is "CreatedBy")
            {
                property.IsNullable = false;
                property.SetDefaultValue("Auto");
            }
            if (property.Name is "LastModifiedBy")
            {
                property.IsNullable = true;
            }
        }

        foreach (var property in builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties())
            .Where(p => p.Name is "Created" or "LastModified"))
        {
            property.SetColumnType("datetime2(0)");
            if (property.Name is "Created")
            {
                property.IsNullable = false;
                property.SetDefaultValueSql("getdate()");
            }
            if (property.Name is "LastModified")
            {
                property.IsNullable = true;
            }
        }

        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ParkingDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<string>().HaveMaxLength(64);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>().ToList())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    //entry.Entity.CreatedBy = _currentUserService.UserName;
                    entry.Entity.CreatedAt = DateTime.Now;
                    //entry.Entity.ModifiedBy = _currentUserService.UserName;
                    //entry.Entity.ModifiedOn = DateTime.Now;
                    break;
                case EntityState.Modified:
                    //entry.Entity.ModifiedBy = _currentUserService.UserName;
                    //entry.Entity.ModifiedOn = DateTime.Now;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }


    
}
