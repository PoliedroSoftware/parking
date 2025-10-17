using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CleanArchitecture.Blazor.Domain.Enums;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class GateConfiguration : IEntityTypeConfiguration<Gate>
{
    public void Configure(EntityTypeBuilder<Gate> builder)
    {
 

        builder.Property(x => x.Name).HasMaxLength(64);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasOne(x => x.Zone).WithMany(z => z.Gates).HasForeignKey(x => x.ZoneId).IsRequired();
        builder.Property(x => x.GateType).HasMaxLength(16).HasConversion<string>().IsRequired();
        builder.Property(x => x.HourlyPermitTypes).HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Web),
                v => JsonSerializer.Deserialize<List<PermitTypes>>(v, JsonSerializerOptions.Web),
                new ValueComparer<List<PermitTypes>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));

        builder.Property(x => x.MonthlyPermitTypes).HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Web),
                v => JsonSerializer.Deserialize<List<PermitTypes>>(v, JsonSerializerOptions.Web),
                new ValueComparer<List<PermitTypes>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));


        builder.Property(t => t.Description).HasMaxLength(512);

        builder.HasOne(x => x.Zone).WithMany(z => z.Gates).HasForeignKey(x => x.ZoneId).IsRequired(); // .OnDelete(DeleteBehavior.Restrict);

    }
}
