using Pp.Data.Domain;
using Pp.Data.GenericRepository;

namespace Pp.Data.UnitOfWork;

public interface IUnitOfWork
{
    Task Complete();
    Task CompleteWithTransaction();
    IGenericRepository<Cart> CartRepository { get; }
    IGenericRepository<CartItem> CartItemRepository { get; }
    IGenericRepository<Category> CategoryRepository { get; }

    IGenericRepository<Product> ProductRepository { get; }
    IGenericRepository<ProductCategory> ProductCategoryRepository { get; }
    IGenericRepository<User> UserRepository { get; }
    IGenericRepository<Wallet> WalletRepository { get; }
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();

}