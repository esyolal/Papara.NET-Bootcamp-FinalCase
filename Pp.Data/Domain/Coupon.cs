using Pp.Base.Entity;
namespace Pp.Data.Domain
{
    public class Coupon : BaseEntity
    {
        public string Code { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime ValidityStartDate { get; set; }
        public DateTime ValidityEndDate { get; set; }
        public long UsedByUserId { get; set; }

    }
}