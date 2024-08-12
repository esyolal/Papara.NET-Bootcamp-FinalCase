namespace Pp.Schema
{
    public class CartRequest
    {
        public List<CartItemRequest> Items { get; set; }
    }
    public class CartResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<CartItemResponse> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
