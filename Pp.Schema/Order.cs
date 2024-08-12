namespace Pp.Schema
{
    public class OrderRequest
    {
        public bool UsePoints { get; set; }
        public bool UseCoupon { get; set; }
    }

    public class OrderResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; } 
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal CartTotal { get; set; } 
        public decimal CouponDiscount { get; set; } 
        public string CouponCode { get; set; }  
        public decimal RewardPointsUsed { get; set; } 

        public List<OrderDetailResponse> OrderDetails { get; set; }
    }
}