using Pp.Base.Entity;

namespace Pp.Data.Domain
{
    public class CartItem : BaseEntity
    {
        public long CartId { get; set; }
        public Cart Cart { get; set; }

        public long ProductId { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal TotalPrice => Quantity * Price;
    }
}
