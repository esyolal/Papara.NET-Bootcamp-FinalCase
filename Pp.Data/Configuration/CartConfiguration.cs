using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pp.Data.Domain;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(c => c.Id);


        builder.HasOne<User>()
               .WithOne()
               .HasForeignKey<Cart>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);


        builder.HasMany(c => c.Items)
               .WithOne(ci => ci.Cart)
               .HasForeignKey(ci => ci.CartId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.TotalCost)
       .HasColumnType("decimal(18,2)")
       .IsRequired()
       .HasDefaultValue(0);
    }
}
