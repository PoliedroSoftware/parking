

namespace ACE.PMS.Infrastructure.EntityConfigurations;
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.LicensePlate).HasMaxLength(16).IsRequired();
        builder.Property(e => e.CardId).HasMaxLength(32).IsRequired();

        builder.Property(e => e.StartDate).HasColumnType("datetime2(0)").IsRequired();
        builder.Property(e => e.ExpiryDate).HasColumnType("datetime2(0)").IsRequired();

        builder.Property(e => e.SpaceType).HasConversion<string>().IsRequired();
        builder.Property(e => e.SpaceNo).HasMaxLength(16); 

        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(32);
        builder.Property(e => e.Email).HasMaxLength(64);
        builder.Property(e => e.Address).HasMaxLength(256);
        builder.Property(e => e.Notes).HasMaxLength(512);

        builder.HasIndex(e => e.LicensePlate).IsUnique().HasDatabaseName("IX_Members_LicensePlate");
        builder.HasIndex(e => e.CardId).IsUnique().HasDatabaseName("IX_Members_CardId");                
                
        builder.HasOne(e => e.Vehicle).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
                
        builder.HasOne(e => e.SpaceGroup).WithMany().IsRequired(false).OnDelete(DeleteBehavior.SetNull);

    }
}
