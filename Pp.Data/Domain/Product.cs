using Pp.Base.Entity;
using System.Collections.Generic;

namespace Pp.Data.Domain
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Features { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public decimal PointsPercentage { get; set; }
        public decimal MaxPoints { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
