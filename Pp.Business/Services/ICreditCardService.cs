using System;

namespace Pp.Business.Services
{
    public interface ICreditCardService
    {
        CreditCardInfo GenerateCreditCard(long userId);
        bool TryDebit(long userId, string cardNumber, string expiryDate, string cvv, decimal amount);
        void Refund(long userId, string cardNumber, decimal amount);
    }
}
