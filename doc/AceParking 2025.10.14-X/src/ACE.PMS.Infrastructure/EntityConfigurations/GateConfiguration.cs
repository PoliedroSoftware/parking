
namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class GateConfiguration : IEntityTypeConfiguration<Gate>
{
    public void Configure(EntityTypeBuilder<Gate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(64);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.ZoneId).IsRequired();
        builder.Property(x => x.GateType).HasMaxLength(16).HasConversion<string>().IsRequired();

        builder.Property(x => x.HourlyPermitTypes).HasMaxLength(256).IsRequired();
        builder.Property(x => x.MonthlyPermitTypes).HasMaxLength(256).IsRequired();

        builder.Property(t => t.Description).HasMaxLength(512);

        builder.HasOne(x => x.Zone).WithMany(z => z.Gates).HasForeignKey(x => x.ZoneId).IsRequired(); // .OnDelete(DeleteBehavior.Restrict);

    }
}
