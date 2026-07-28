using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name_En).HasMaxLength(128).IsRequired().IsUnicode(false);
        builder.Property(e => e.Name_Tc).HasMaxLength(128).IsRequired();

        builder.HasIndex(e => e.Date).IsUnique().HasDatabaseName("IX_Holidays_Date");

        builder.Ignore(e => e.DomainEvents);
    }
}
