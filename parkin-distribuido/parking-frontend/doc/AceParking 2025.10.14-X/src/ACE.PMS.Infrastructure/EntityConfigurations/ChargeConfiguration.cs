
using ACE.PMS.Infrastructure.Conversions;

namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(t=>t.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.Id, "IX_Charge_Id").IsUnique();

        builder.Property(t => t.Name).HasMaxLength(64).IsRequired();
        builder.Property(t => t.EffectiveDate).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(t => t.BeforeContent).HasMaxLength(int.MaxValue).IsRequired().HasJsonConversion();
        builder.Property(t => t.AfterContent).HasMaxLength(int.MaxValue).IsRequired().HasJsonConversion();
        builder.Property(t => t.Description).HasMaxLength(512);        
    }
}
