using Pp.Base.Entity;
namespace Pp.Data.Domain;
public class Cart : BaseEntity
{
    public long UserId { get; set; } 

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>(); 

     public decimal TotalCost { get; set; } 
}

