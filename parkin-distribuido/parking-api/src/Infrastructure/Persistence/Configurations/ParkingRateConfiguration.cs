using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ParkingRateConfiguration : IEntityTypeConfiguration<ParkingRate>
{
    public void Configure(EntityTypeBuilder<ParkingRate> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.VehicleType).IsRequired();
        builder.Property(e => e.HourlyRate).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.DayRate).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.NightRate).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(512);

        builder.HasIndex(e => new { e.VehicleType, e.Name });
        builder.Ignore(e => e.DomainEvents);
    }
}
