using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pp.Data.Domain;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Url)
            .HasMaxLength(200);

        builder.Property(c => c.Tags)
            .HasMaxLength(500);

        builder.HasMany(c => c.ProductCategories)
            .WithOne(pc => pc.Category)
            .HasForeignKey(pc => pc.CategoryId);
    }
}
