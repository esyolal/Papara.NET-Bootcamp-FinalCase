using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Pp.Business.Command;
using Pp.Business.Cqrs;
using Pp.Business.Services;
using Pp.Data.Domain;
using Pp.Data.GenericRepository;
using Pp.Data.UnitOfWork;

namespace Pp.Tests;

public class CheckoutTests
{
    private const long UserId = 7;
    private const long CartId = 12;
    private Mock<IUnitOfWork> unitOfWork = null!;
    private Mock<IGenericRepository<Cart>> carts = null!;
    private Mock<IGenericRepository<Wallet>> wallets = null!;
    private Cart cart = null!;
    private Wallet wallet = null!;
    private ProcessPaymentCommandHandler handler = null!;

    [SetUp]
    public void SetUp()
    {
        cart = new Cart
        {
            Id = CartId, UserId = UserId, TotalCost = 25m,
            Items = [new CartItem { ProductId = 42, Price = 25m, Quantity = 1 }]
        };
        wallet = new Wallet { UserId = UserId, Balance = 100m };
        carts = new Mock<IGenericRepository<Cart>>();
        wallets = new Mock<IGenericRepository<Wallet>>();
        carts.Setup(repository => repository.FirstOrDefault(It.IsAny<Expression<Func<Cart, bool>>>(), nameof(Cart.Items)))
            .ReturnsAsync((Expression<Func<Cart, bool>> predicate, string[] _) =>
                new[] { cart }.FirstOrDefault(predicate.Compile())!);
        wallets.Setup(repository => repository.FirstOrDefault(It.IsAny<Expression<Func<Wallet, bool>>>()))
            .ReturnsAsync((Expression<Func<Wallet, bool>> predicate, string[] _) =>
                new[] { wallet }.FirstOrDefault(predicate.Compile())!);

        unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(unit => unit.CartRepository).Returns(carts.Object);
        unitOfWork.SetupGet(unit => unit.WalletRepository).Returns(wallets.Object);
        unitOfWork.SetupGet(unit => unit.OrderRepository).Returns(Mock.Of<IGenericRepository<OrderRecord>>());
        var products = new Mock<IGenericRepository<Product>>();
        products.Setup(repository => repository.Find(It.IsAny<Expression<Func<Product, bool>>>()))
            .ReturnsAsync([new Product { Id = 42, PointsPercentage = 10m, MaxPoints = 10m }]);
        unitOfWork.SetupGet(unit => unit.ProductRepository).Returns(products.Object);
        unitOfWork.Setup(unit => unit.Complete()).Returns(Task.CompletedTask);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(user => user.GetCurrentUserId()).Returns(UserId);
        handler = new ProcessPaymentCommandHandler(unitOfWork.Object, currentUser.Object);
    }

    [Test]
    public async Task CheckoutDebitsWalletAndCommitsAfterSaveCompletes()
    {
        var save = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        unitOfWork.Setup(unit => unit.Complete()).Returns(save.Task);

        var payment = handler.Handle(new ProcessPaymentCommand { CartId = CartId }, CancellationToken.None);
        unitOfWork.Verify(unit => unit.Complete(), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransaction(), Times.Never);
        save.SetResult();
        var result = await payment;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Payment successful"));
            Assert.That(wallet.Balance, Is.EqualTo(75m));
            Assert.That(wallet.Points, Is.EqualTo(2.50m));
        });
        unitOfWork.Verify(unit => unit.BeginTransaction(), Times.Once);
        wallets.Verify(repository => repository.Update(wallet), Times.Once);
        carts.Verify(repository => repository.Delete(cart), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransaction(), Times.Once);
        unitOfWork.Verify(unit => unit.RollbackTransaction(), Times.Never);
    }

    [TestCase(99L, UserId, 25, UserId, 100, "Cart not found")]
    [TestCase(CartId, 99L, 25, UserId, 100, "Cart not found")]
    [TestCase(CartId, UserId, 0, UserId, 100, "Cart is empty")]
    [TestCase(CartId, UserId, 25, 99L, 100, "Wallet not found")]
    [TestCase(CartId, UserId, 25, UserId, 24, "Insufficient balance")]
    public async Task RejectedCheckoutDoesNotWrite(
        long requestedCartId, long cartOwnerId, int total, long walletOwnerId, int balance, string message)
    {
        cart.UserId = cartOwnerId;
        cart.TotalCost = total;
        cart.Items.Single().Price = total;
        wallet.UserId = walletOwnerId;
        wallet.Balance = balance;

        var result = await handler.Handle(
            new ProcessPaymentCommand { CartId = requestedCartId }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Message, Is.EqualTo(message));
            Assert.That(wallet.Balance, Is.EqualTo(balance));
        });
        unitOfWork.Verify(unit => unit.BeginTransaction(), Times.Never);
        unitOfWork.Verify(unit => unit.Complete(), Times.Never);
        wallets.Verify(repository => repository.Update(It.IsAny<Wallet>()), Times.Never);
        carts.Verify(repository => repository.Delete(It.IsAny<Cart>()), Times.Never);
    }

    [Test]
    public void CheckoutRollsBackWhenSaveFails()
    {
        var saveError = new InvalidOperationException("Save failed");
        unitOfWork.Setup(unit => unit.Complete()).ThrowsAsync(saveError);

        var error = Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(new ProcessPaymentCommand { CartId = CartId }, CancellationToken.None));

        Assert.That(error, Is.SameAs(saveError));
        unitOfWork.Verify(unit => unit.BeginTransaction(), Times.Once);
        unitOfWork.Verify(unit => unit.RollbackTransaction(), Times.Once);
        unitOfWork.Verify(unit => unit.CommitTransaction(), Times.Never);
    }

    [Test]
    public async Task CheckoutRecordsCouponDiscountAndPointsUsed()
    {
        var now = DateTime.UtcNow;
        var coupon = new Coupon
        {
            Code = "SAVE10", DiscountAmount = 10m, IsActive = true,
            ValidityStartDate = now.AddDays(-1), ValidityEndDate = now.AddDays(1)
        };
        var coupons = new Mock<IGenericRepository<Coupon>>();
        coupons.Setup(repository => repository.FirstOrDefault(It.IsAny<Expression<Func<Coupon, bool>>>()))
            .ReturnsAsync((Expression<Func<Coupon, bool>> predicate, string[] _) =>
                new[] { coupon }.FirstOrDefault(predicate.Compile())!);
        unitOfWork.SetupGet(unit => unit.CouponRepository).Returns(coupons.Object);
        var orders = new Mock<IGenericRepository<OrderRecord>>();
        unitOfWork.SetupGet(unit => unit.OrderRepository).Returns(orders.Object);
        wallet.Points = 5m;

        var result = await handler.Handle(
            new ProcessPaymentCommand { CartId = CartId, CouponCode = " save10 ", UsePoints = true },
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(wallet.Balance, Is.EqualTo(90m));
        Assert.That(wallet.Points, Is.EqualTo(1m));
        Assert.That(coupon.UsedByUserId, Is.EqualTo(UserId));
        orders.Verify(repository => repository.Add(It.Is<OrderRecord>(order =>
            order.UserId == UserId && order.CouponCode == "SAVE10" &&
            order.CouponDiscount == 10m && order.PointsUsed == 5m && order.AmountPaid == 10m)), Times.Once);
    }
}
