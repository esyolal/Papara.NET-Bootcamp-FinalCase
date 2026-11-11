using System.Linq.Expressions;
using AutoMapper;
using Moq;
using NUnit.Framework;
using Pp.Business.Cqrs;
using Pp.Business.Mapper;
using Pp.Business.Query;
using Pp.Data.Domain;
using Pp.Data.GenericRepository;
using Pp.Data.UnitOfWork;

namespace Pp.Tests;

public class CartQueryTests
{
    [TestCase(7L, true)]
    [TestCase(8L, false)]
    public async Task GetCartLoadsItemsForTheRequestedUser(long userId, bool expectedSuccess)
    {
        var cart = new Cart
        {
            Id = 12,
            UserId = 7,
            Items = [new CartItem { ProductId = 42, Quantity = 2, Price = 10m }]
        };
        var carts = new Mock<IGenericRepository<Cart>>(MockBehavior.Strict);
        carts.Setup(repository => repository.FirstOrDefault(
                It.IsAny<Expression<Func<Cart, bool>>>(), nameof(Cart.Items)))
            .ReturnsAsync((Expression<Func<Cart, bool>> predicate, string[] _) =>
                new[] { cart }.FirstOrDefault(predicate.Compile())!);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(unit => unit.CartRepository).Returns(carts.Object);
        var mapper = new MapperConfiguration(config => config.AddProfile<MapperConfig>()).CreateMapper();
        var handler = new CartQueryHandler(unitOfWork.Object, mapper);

        var result = await handler.Handle(new GetCartByUserIdQuery(userId), CancellationToken.None);

        Assert.That(result.IsSuccess, Is.EqualTo(expectedSuccess));
        if (expectedSuccess)
        {
            Assert.That(result.Data.UserId, Is.EqualTo(userId));
            Assert.That(result.Data.Items.Single().ProductId, Is.EqualTo(42));
            Assert.That(result.Data.Items.Single().Quantity, Is.EqualTo(2));
            Assert.That(result.Data.TotalAmount, Is.EqualTo(10m));
        }
        else
        {
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Message, Is.EqualTo("Cart not found"));
        }
    }
}
