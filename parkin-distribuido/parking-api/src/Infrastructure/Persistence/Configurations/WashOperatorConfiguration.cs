using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class WashOperatorConfiguration : IEntityTypeConfiguration<WashOperator>
{
    public void Configure(EntityTypeBuilder<WashOperator> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(120).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => e.Name);
        builder.Ignore(e => e.DomainEvents);
    }
}
