namespace Pp.Schema
{
    public class OrderDetailRequest
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductDetails { get; set; }
    }

    public class OrderDetailResponse
    {
        public long Id { get; set; }
        public long OrderId { get; set; } 
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int PointsUsed { get; set; }
        public bool IsCouponUsed { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; }  
    }

}