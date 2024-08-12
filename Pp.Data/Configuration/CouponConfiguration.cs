using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pp.Data.Domain;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UsedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
