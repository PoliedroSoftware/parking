using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;


public class SpaceGroupConfiguration : IEntityTypeConfiguration<SpaceGroup>
{
    public void Configure(EntityTypeBuilder<SpaceGroup> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.Name).HasMaxLength(64).IsRequired();

        builder.Property(e => e.Description).HasMaxLength(256);

        builder.HasIndex(e => new { e.Name }).IsUnique(); 

        builder.HasOne(e => e.Zone).WithMany(x=>x.SpaceGroups).HasForeignKey(x=>x.ZoneId);
        builder.HasOne(x => x.Member).WithMany(x => x.SpaceGroups).HasForeignKey(x => x.MemberId);
    }
}
