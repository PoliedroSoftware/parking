namespace ACE.PMS.Infrastructure.EntityConfigurations;

public class CarparkConfiguration : IEntityTypeConfiguration<Carpark>
{
    public void Configure(EntityTypeBuilder<Carpark> builder)
    {
        builder.HasKey(x => x.Id);
        //builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(e => e.AppKey).HasMaxLength(256).IsRequired();
        builder.Property(e => e.MachineCode).HasMaxLength(512);
        builder.Property(e => e.RegistrationCode).HasMaxLength(512);

        builder.OwnsOne(o => o.Name, n =>
        {
            n.Property(e => e.Code).HasMaxLength(16).IsRequired();
            n.Property(e => e.En).HasMaxLength(128).IsRequired().IsUnicode(false);
            n.Property(e => e.Tc).HasMaxLength(128).IsRequired();
        });

        builder.OwnsOne(o => o.CompanyName, n =>
        {
            n.Property(e => e.En).HasMaxLength(256).IsUnicode(false);
            n.Property(e => e.Tc).HasMaxLength(256);
        });

        builder.OwnsOne(o => o.Address, n =>
        {
            n.Property(e => e.En).HasMaxLength(512).IsUnicode(false);
            n.Property(e => e.Tc).HasMaxLength(512);
        });
        
        builder.Property(e => e.Description).HasMaxLength(512);                
    }
}