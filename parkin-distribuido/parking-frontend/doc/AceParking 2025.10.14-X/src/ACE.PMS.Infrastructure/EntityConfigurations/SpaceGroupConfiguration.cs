

namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class SpaceGroupConfiguration : IEntityTypeConfiguration<SpaceGroup>
{
    public void Configure(EntityTypeBuilder<SpaceGroup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();

        builder.Property(e => e.Description).HasMaxLength(256);
        
        builder.HasIndex(e => new { e.Name }).IsUnique(); //月租組別名稱不可重複

        builder.HasOne(e => e.Zone).WithMany().IsRequired(); // .OnDelete(DeleteBehavior.Restrict);        
    }
}
