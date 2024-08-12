using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pp.Data.Domain;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Quantity)
               .IsRequired();

        builder.Property(ci => ci.Price)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(ci => ci.ProductId)
               .IsRequired();

        builder.HasOne(ci => ci.Cart)
               .WithMany(c => c.Items)
               .HasForeignKey(ci => ci.CartId)
               .OnDelete(DeleteBehavior.Cascade);

    }
}
