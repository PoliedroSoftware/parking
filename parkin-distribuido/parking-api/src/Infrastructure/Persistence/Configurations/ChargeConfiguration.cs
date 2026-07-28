using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {


        builder.Property(t => t.Name).HasMaxLength(64).IsRequired();
        builder.Property(t => t.EffectiveDate).IsRequired();
        builder.Property(t => t.BeforeContent).HasMaxLength(int.MaxValue).HasJsonConversion();
        builder.Property(t => t.AfterContent).HasMaxLength(int.MaxValue).HasJsonConversion();
        builder.Property(t => t.Description).HasMaxLength(512);

        builder.Ignore(e => e.DomainEvents);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
    }
}
