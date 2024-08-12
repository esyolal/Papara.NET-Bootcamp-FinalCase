namespace Pp.Schema
{

    public class CartItemRequest
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
       
        public decimal Price { get; set; }

    }


    public class CartItemResponse
    {

        public long Id { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
