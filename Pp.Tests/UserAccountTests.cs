using AutoMapper;
using Moq;
using NUnit.Framework;
using Pp.Business.Command;
using Pp.Business.Cqrs;
using Pp.Business.Mapper;
using Pp.Business.Services;
using Pp.Business.Token;
using Pp.Data.Domain;
using Pp.Data.GenericRepository;
using Pp.Data.UnitOfWork;
using Pp.Schema;

namespace Pp.Tests;

public class UserAccountTests
{
    [Test]
    public async Task UpdatingPasswordStoresHashAndPreservesRole()
    {
        var user = new User { Id = 7, Role = "User", Password = "legacy value" };
        var users = new Mock<IGenericRepository<User>>();
        users.Setup(repository => repository.GetById(7)).ReturnsAsync(user);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(unit => unit.UserRepository).Returns(users.Object);
        unitOfWork.Setup(unit => unit.Complete()).Returns(Task.CompletedTask);
        var handler = new UserCommandHandler(unitOfWork.Object, Mock.Of<IMapper>(),
            Mock.Of<ITokenService>(), Mock.Of<ICurrentUser>());
        const string password = "example-test-password";

        var result = await handler.Handle(
            new UpdateUserCommand(7, new UpdateUserRequest("Test", "User", "test@example.invalid", "", password)),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(user.Password, Is.Not.EqualTo(password));
        Assert.That(BCrypt.Net.BCrypt.Verify(password, user.Password), Is.True);
        Assert.That(user.Role, Is.EqualTo("User"));
        unitOfWork.Verify(unit => unit.Complete(), Times.Once);
    }

    [TestCase(typeof(RegisterResponse))]
    [TestCase(typeof(UserResponse))]
    public void AccountResponsesDoNotExposePassword(Type responseType)
    {
        Assert.That(responseType.GetProperty("Password"), Is.Null);
    }
}
