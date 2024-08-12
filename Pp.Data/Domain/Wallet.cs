using Pp.Base.Entity;

namespace Pp.Data.Domain
{
    public class Wallet : BaseEntity
    {
        public long UserId { get; set; }
        public decimal Balance { get; set; } 
        public decimal Points { get; set; } 
        public User User { get; set; }
    }
}
