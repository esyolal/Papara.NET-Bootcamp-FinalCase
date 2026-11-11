namespace Pp.Business.Services
{
    public class CreditCardService : ICreditCardService
    {
        private const decimal CardLimit = 10000M;

        private readonly Dictionary<string, TestCard> cards = new();
        private readonly object cardLock = new();

        public CreditCardInfo GenerateCreditCard(long userId)
        {
            var expires = DateTime.UtcNow.AddYears(2);
            var endOfMonth = new DateTime(expires.Year, expires.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            var card = new CreditCardInfo
            {
                CardNumber = $"TEST-{Guid.NewGuid():N}",
                ExpiryDate = expires.ToString("MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                CVV = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100, 1000).ToString()
            };

            lock (cardLock)
            {
                cards.Add(card.CardNumber, new TestCard(userId, card.ExpiryDate, card.CVV, endOfMonth));
            }
            return card;
        }

        public bool TryDebit(long userId, string cardNumber, string expiryDate, string cvv, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || amount <= 0 || decimal.Round(amount, 2) != amount)
            {
                return false;
            }

            lock (cardLock)
            {
                if (!cards.TryGetValue(cardNumber, out var card) || card.UserId != userId ||
                    card.ExpiryDate != expiryDate || card.Cvv != cvv ||
                    card.ExpiresAt <= DateTime.UtcNow || card.Balance < amount)
                {
                    return false;
                }

                card.Balance -= amount;
                return true;
            }
        }

        public void Refund(long userId, string cardNumber, decimal amount)
        {
            lock (cardLock)
            {
                if (cards.TryGetValue(cardNumber, out var card) && card.UserId == userId)
                {
                    card.Balance += amount;
                }
            }
        }

        private sealed record TestCard(long UserId, string ExpiryDate, string Cvv, DateTime ExpiresAt)
        {
            public decimal Balance { get; set; } = CardLimit;
        }
    }

    public class CreditCardInfo
    {
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}
