using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.LicensePlate).HasMaxLength(16).IsRequired();
        builder.Property(e => e.CardId).HasMaxLength(32).IsRequired();

        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.ExpiryDate).IsRequired();

        builder.Property(e => e.SpaceType).HasJsonConversion();
        builder.Property(e => e.SpaceNo).HasMaxLength(16);

        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(32);
        builder.Property(e => e.Email).HasMaxLength(64);
        builder.Property(e => e.Address).HasMaxLength(256);
        builder.Property(e => e.Notes).HasMaxLength(512);

        builder.HasIndex(e => e.LicensePlate).IsUnique();
        builder.HasIndex(e => e.CardId).IsUnique();

        builder.Ignore(e => e.DomainEvents);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);

    }
}
