using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(64);

        builder.Property(x => x.ServiceCategoryId).HasMaxLength(16).HasConversion<string>(); 
        builder.Property(x => x.VehicleTypeId).HasMaxLength(16).HasConversion<string>();

        builder.HasIndex(e => new { e.Name }).IsUnique();

        builder.HasOne(x => x.Zone).WithMany(x => x.Vehicles).HasForeignKey(x => x.ZoneId);

        builder.HasOne(x => x.Charge).WithMany().HasForeignKey(x=> x.ChargeId);

        builder.Ignore(x => x.Occupied);
    }
}
