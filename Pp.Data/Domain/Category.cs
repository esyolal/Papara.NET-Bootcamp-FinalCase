using Pp.Base.Entity;

namespace Pp.Data.Domain
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Tags { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
