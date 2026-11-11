using Pp.Base.Entity;

namespace Pp.Data.Domain;

public class OrderRecord : BaseEntity
{
    public long UserId { get; set; }
    public decimal Subtotal { get; set; }
    public string? CouponCode { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal PointsUsed { get; set; }
    public decimal AmountPaid { get; set; }
    public string ItemsJson { get; set; } = "[]";
}
