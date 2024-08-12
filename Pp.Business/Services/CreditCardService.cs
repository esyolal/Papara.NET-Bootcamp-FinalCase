namespace Pp.Business.Services
{
    public class CreditCardService : ICreditCardService
    {
        private const decimal CardLimit = 10000M;

        private static readonly Dictionary<string, CreditCardInfo> StoredCreditCards = new Dictionary<string, CreditCardInfo>();

        public CreditCardInfo GenerateCreditCard()
        {
            Random random = new Random();
            string cardNumber = $"{random.Next(1000, 9999)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}-{random.Next(1000, 9999)}";
            string expiryDate = $"{random.Next(1, 12).ToString("D2")}/{random.Next(DateTime.Now.Year, DateTime.Now.Year + 5)}";
            string cvv = random.Next(100, 999).ToString();

            var creditCardInfo = new CreditCardInfo
            {
                CardNumber = cardNumber,
                ExpiryDate = expiryDate,
                CVV = cvv
            };

            StoredCreditCards[cardNumber] = creditCardInfo;

            return creditCardInfo;
        }

        public decimal GetCardLimit()
        {
            return CardLimit;
        }

        public bool ValidateTransaction(decimal amount)
        {
            return amount <= CardLimit;
        }

        public bool ValidateCreditCard(string cardNumber, string expiryDate, string cvv)
        {
            if (StoredCreditCards.TryGetValue(cardNumber, out var storedCard))
            {
                return storedCard.ExpiryDate == expiryDate && storedCard.CVV == cvv;
            }
            return false;
        }
    }

    public class CreditCardInfo
    {
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}
