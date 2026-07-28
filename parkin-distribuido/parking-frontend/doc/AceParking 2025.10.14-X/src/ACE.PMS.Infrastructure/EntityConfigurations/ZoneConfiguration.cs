
using ACE.PMS.Infrastructure.Conversions;

namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.OwnsOne(o => o.Name, n =>
        {
            n.Property(e => e.Code).HasMaxLength(16).IsRequired();
            n.Property(e => e.En).HasMaxLength(128).IsRequired().IsUnicode(false);
            n.Property(e => e.Tc).HasMaxLength(128).IsRequired();
        });

        builder.Property(e => e.HolidaySets).HasMaxLength(64).IsRequired().HasDefaultValue("1,0,0,0,0,0,1,1");
        builder.Property(e => e.Description).HasMaxLength(512);

        builder.Property(e => e.HourlySets).HasMaxLength(1024).HasJsonConversion();
        builder.Property(e => e.MonthlySets).HasMaxLength(1024).HasJsonConversion();

        builder.Navigation(e=> e.Vehicles).AutoInclude();

        builder.HasOne(e => e.Carpark).WithMany(x => x.Zones).IsRequired().HasForeignKey(e => e.CarparkId).OnDelete(DeleteBehavior.Restrict);
    }
}