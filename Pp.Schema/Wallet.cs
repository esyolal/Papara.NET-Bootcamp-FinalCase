using System.Text.Json.Serialization;

namespace Pp.Schema
{
    public class WalletRequest
    {
        public decimal Amount { get; set; }
        public string CreditCardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }

    public class WalletResponse
    {
        [JsonIgnore]
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal Balance { get; set; }
        public decimal Points { get; set; }
    }
}