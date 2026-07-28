using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence.Configurations;

public class MemberRentalConfiguration : IEntityTypeConfiguration<MemberRental>
{
    public void Configure(EntityTypeBuilder<MemberRental> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.LicensePlate).HasMaxLength(16).IsRequired();
        builder.Property(e => e.CardId).HasMaxLength(32).IsRequired();

        builder.Property(e => e.StartDate).IsRequired();
        builder.Property(e => e.ExpiryDate).IsRequired();

        builder.Property(e => e.RentalFee).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.Deposit).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.AmountDue).HasColumnType("decimal(9,1)").IsRequired();
        builder.Property(e => e.AmountPaid).HasColumnType("decimal(9,1)").IsRequired();

        builder.Property(e => e.PaymentMethodId).HasConversion<string>();

        builder.Property(e => e.Notes).HasMaxLength(512);
        builder.HasOne(e => e.Member)
            .WithMany(m => m.MemberRentals)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.SetNull);
        builder.HasIndex(e => e.LicensePlate);
        builder.HasIndex(e => e.CardId);
    }
}
