using Pp.Base.Entity;

namespace Pp.Data.Domain
{
    public class ProductCategory : BaseEntity
    {
        public long ProductId { get; set; }
        public Product Product { get; set; }

        public long CategoryId { get; set; }
        public Category Category { get; set; }
    }
}