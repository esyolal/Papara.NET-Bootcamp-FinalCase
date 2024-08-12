using Microsoft.EntityFrameworkCore.Storage;
using Pp.Data.Context;
using Pp.Data.Domain;
using Pp.Data.GenericRepository;

namespace Pp.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly AppDbContext dbContext;
    
    private IDbContextTransaction _transaction;

    public IGenericRepository<Cart> CartRepository { get; }
    public IGenericRepository<CartItem> CartItemRepository { get; }
    public IGenericRepository<Category> CategoryRepository { get; }
    public IGenericRepository<Product> ProductRepository { get; }
    public IGenericRepository<ProductCategory> ProductCategoryRepository { get; }

    public IGenericRepository<Wallet> WalletRepository { get; }
    public IGenericRepository<User> UserRepository { get; }



    public UnitOfWork(AppDbContext dbContext)
    {
        this.dbContext = dbContext;

        CartRepository = new GenericRepository<Cart>(this.dbContext);
        CartItemRepository = new GenericRepository<CartItem>(this.dbContext);
        CategoryRepository = new GenericRepository<Category>(this.dbContext);
        UserRepository = new GenericRepository<User>(this.dbContext);
        ProductRepository = new GenericRepository<Product>(this.dbContext);
        ProductCategoryRepository = new GenericRepository<ProductCategory>(this.dbContext);
        WalletRepository = new GenericRepository<Wallet>(this.dbContext);
    }

   

    public async Task Complete()
    {
        await dbContext.SaveChangesAsync();
    }

    public async Task CompleteWithTransaction()
    {
        using (var dbTransaction = await dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                await dbContext.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                Console.WriteLine(ex);
                throw;
            }
        }
    }
    public void BeginTransaction()
    {
        _transaction = dbContext.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }
     public void Dispose()
    {
        dbContext.Dispose();
    }
}