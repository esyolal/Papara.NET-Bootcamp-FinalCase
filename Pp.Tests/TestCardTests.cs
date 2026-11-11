using NUnit.Framework;
using Pp.Business.Services;

namespace Pp.Tests;

public class TestCardTests
{
    [Test]
    public void TestCardEnforcesOwnershipAndCumulativeLimit()
    {
        var cards = new CreditCardService();
        var card = cards.GenerateCreditCard(7);

        Assert.That(cards.TryDebit(8, card.CardNumber, card.ExpiryDate, card.CVV, 1m), Is.False);
        Assert.That(cards.TryDebit(7, card.CardNumber, card.ExpiryDate, card.CVV, -1m), Is.False);
        Assert.That(cards.TryDebit(7, card.CardNumber, card.ExpiryDate, card.CVV, 6000m), Is.True);
        Assert.That(cards.TryDebit(7, card.CardNumber, card.ExpiryDate, card.CVV, 6000m), Is.False);
        cards.Refund(7, card.CardNumber, 6000m);
        Assert.That(cards.TryDebit(7, card.CardNumber, card.ExpiryDate, card.CVV, 10000m), Is.True);
    }
}
