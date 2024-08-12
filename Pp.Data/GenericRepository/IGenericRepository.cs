using System.Linq.Expressions;

namespace Pp.Data.GenericRepository;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task Save();
    Task<TEntity?> GetById(long Id, params string[] includes);
    Task InsertAsync(TEntity entity, string Name);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task Delete(long Id);
    Task<List<TEntity>> GetAll(params string[] includes);
    Task<List<TEntity>> Where(Expression<Func<TEntity, bool>> expression, params string[] includes);
    Task<TEntity> FirstOrDefault(Expression<Func<TEntity, bool>> expression, params string[] includes);
    Task<List<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);

    Task DeleteRange(IEnumerable<TEntity> entities);

}