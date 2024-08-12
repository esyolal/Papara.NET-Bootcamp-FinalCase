using System;

namespace Pp.Business.Services
{
    public interface ICreditCardService
    {
        CreditCardInfo GenerateCreditCard();
        decimal GetCardLimit();
        bool ValidateTransaction(decimal amount);
        bool ValidateCreditCard(string cardNumber, string expiryDate, string cvv);
    }
}
