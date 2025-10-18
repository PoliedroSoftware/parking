using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class MemberVehicleConfiguration: IEntityTypeConfiguration<MemberVehicle>
{
    public void Configure(EntityTypeBuilder<MemberVehicle> builder)
    {
    
        builder.HasKey(x => x.Id);
        builder.HasKey(x => new { x.MemberId, x.VehicleId });
        builder.HasOne(mv => mv.Member)
               .WithMany(m => m.MemberVehicles)
               .HasForeignKey(mv => mv.MemberId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(mv => mv.Vehicle)
               .WithMany()
               .HasForeignKey(mv => mv.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
