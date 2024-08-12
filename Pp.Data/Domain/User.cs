using Microsoft.AspNetCore.Identity;
using Pp.Base.Entity;

namespace Pp.Data.Domain
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public Wallet Wallet { get; set; }
        public ICollection<Coupon> CouponsUsed { get; set; }
    }
}
