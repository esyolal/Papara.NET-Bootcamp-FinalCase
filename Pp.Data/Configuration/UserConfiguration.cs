using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pp.Data.Domain;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(u => u.Surname)
            .IsRequired()
            .HasMaxLength(25);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasMaxLength(50);
    }
}
