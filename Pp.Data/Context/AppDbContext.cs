using Microsoft.EntityFrameworkCore;
using Pp.Data.Domain;
namespace Pp.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<OrderRecord> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new CartConfiguration());
            modelBuilder.ApplyConfiguration(new CartItemConfiguration());
            modelBuilder.ApplyConfiguration(new ProductCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new WalletConfiguration());
            modelBuilder.ApplyConfiguration(new CouponConfiguration());
            modelBuilder.Entity<OrderRecord>(order =>
            {
                order.HasIndex(o => new { o.UserId, o.Id });
                order.Property(o => o.CouponCode).HasMaxLength(450);
                order.Property(o => o.Subtotal).HasPrecision(18, 2);
                order.Property(o => o.CouponDiscount).HasPrecision(18, 2);
                order.Property(o => o.PointsUsed).HasPrecision(18, 2);
                order.Property(o => o.AmountPaid).HasPrecision(18, 2);
            });
        }
    }


}
