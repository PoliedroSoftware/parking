using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class ZoneMemberConfiguration : IEntityTypeConfiguration<ZoneMember>
{
    public void Configure(EntityTypeBuilder<ZoneMember> builder)
    {
        builder.HasKey(x => new { x.ZoneId, x.MemberId });
        builder.HasOne(builder => builder.Zone)
               .WithMany(zone => zone.AllowedMember)
               .HasForeignKey(builder => builder.ZoneId)
               .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
        builder.HasOne(builder => builder.Member)
                .WithMany(member => member.AllowedZone)
                .HasForeignKey(builder => builder.MemberId)
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}
