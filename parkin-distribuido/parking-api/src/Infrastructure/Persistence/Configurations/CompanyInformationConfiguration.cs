using CleanArchitecture.Blazor.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class CompanyInformationConfiguration : IEntityTypeConfiguration<CompanyInformation>
{
    public void Configure(EntityTypeBuilder<CompanyInformation> builder)
    {
        builder.Property(e => e.DisplayName).HasMaxLength(180).IsRequired();
        builder.Property(e => e.TradeName).HasMaxLength(180).IsRequired();
        builder.Property(e => e.TaxId).HasMaxLength(80).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(250).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FooterText).HasMaxLength(250).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.IsActive);
        builder.Ignore(e => e.DomainEvents);
    }
}
