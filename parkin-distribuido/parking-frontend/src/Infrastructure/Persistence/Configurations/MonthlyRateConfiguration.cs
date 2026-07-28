using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class MonthlyRateConfiguration : IEntityTypeConfiguration<MonthlyRate>
{
    public void Configure(EntityTypeBuilder<MonthlyRate> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.VehicleType).IsRequired();
        builder.Property(e => e.MonthlyFee).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.Deposit).HasColumnType("decimal(12,2)").IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(512);

        builder.HasIndex(e => new { e.VehicleType, e.Name });
        builder.Ignore(e => e.DomainEvents);
    }
}
