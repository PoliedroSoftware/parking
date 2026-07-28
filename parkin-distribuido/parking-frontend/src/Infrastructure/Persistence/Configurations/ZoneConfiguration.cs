using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ZoneConfiguration : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.ComplexProperty(x => x.Name);
        builder.Property(e => e.HolidaySets).HasMaxLength(64).IsRequired().HasDefaultValue("1,0,0,0,0,0,1,1");
        builder.Property(e => e.Description).HasMaxLength(512);

        builder.Navigation(e => e.Vehicles).AutoInclude();

        builder.HasOne(e => e.Carpark).WithMany(x => x.Zones).IsRequired().HasForeignKey(e => e.CarparkId).OnDelete(DeleteBehavior.Restrict);
    }
}
