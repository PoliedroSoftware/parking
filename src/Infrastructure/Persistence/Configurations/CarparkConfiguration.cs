using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class CarparkConfiguration : IEntityTypeConfiguration<Carpark>
{
    public void Configure(EntityTypeBuilder<Carpark> builder)
    {


        builder.Property(e => e.AppKey).HasMaxLength(256).IsRequired();
        builder.Property(e => e.MachineCode).HasMaxLength(512);
        builder.Property(e => e.RegistrationCode).HasMaxLength(512);

        builder.ComplexProperty(x => x.Name);
        builder.ComplexProperty(x => x.CompanyName);
        builder.ComplexProperty(x => x.Address);

        

        builder.Property(e => e.Description).HasMaxLength(512);
        builder.Ignore(e => e.DomainEvents);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
    }
}
