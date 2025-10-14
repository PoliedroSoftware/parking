
namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(64);

        builder.Property(x => x.ServiceCategoryId).HasMaxLength(16).HasConversion<string>(); //.IsRequired();
        builder.Property(x => x.VehicleTypeId).HasMaxLength(16).HasConversion<string>();
        
        builder.HasIndex(e => new { e.Name }).IsUnique(); //車類名稱不可重複

        builder.HasOne(x => x.Zone).WithMany(x=>x.Vehicles).IsRequired();
                
        builder.HasOne(x => x.Charge).WithMany().IsRequired(false).OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(x => x.Occupied);
    }
}
