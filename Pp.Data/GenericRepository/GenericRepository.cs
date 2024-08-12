using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Pp.Base.Entity;
using Pp.Data.Context;

namespace Pp.Data.GenericRepository;

internal class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly AppDbContext dbContext;

    public GenericRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task Save()
    {
        await dbContext.SaveChangesAsync();
    }

    public async Task<TEntity?> GetById(long id, params string[] includes)
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        query = includes.Aggregate(query, (current, inc) => current.Include(inc));
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task InsertAsync(TEntity entity,string userEmail)
    {
        entity.IsActive = true;
        entity.InsertDate = DateTime.UtcNow;
        entity.InsertUser = userEmail;
        await dbContext.Set<TEntity>().AddAsync(entity);
        await Save();
    }

    public void Update(TEntity entity)
    {
        dbContext.Set<TEntity>().Update(entity);
    }

    public void Delete(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public async Task Delete(long id)
    {
        var entity = await dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        if (entity != null)
        {
            dbContext.Set<TEntity>().Remove(entity);
            await Save();
        }
    }

    public async Task<List<TEntity>> Where(Expression<Func<TEntity, bool>> expression, params string[] includes)
    {
        var query = dbContext.Set<TEntity>().Where(expression).AsQueryable();
        query = includes.Aggregate(query, (current, inc) => current.Include(inc));
        return await query.ToListAsync();
    }

    public async Task<TEntity?> FirstOrDefault(Expression<Func<TEntity, bool>> expression, params string[] includes)
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        query = includes.Aggregate(query, (current, inc) => current.Include(inc));
        return await query.FirstOrDefaultAsync(expression);
    }

    public async Task<List<TEntity>> GetAll(params string[] includes)
    {
        var query = dbContext.Set<TEntity>().AsQueryable();
        query = includes.Aggregate(query, (current, inc) => current.Include(inc));
        return await query.ToListAsync();
    }

    public async Task<List<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return await dbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await dbContext.Set<TEntity>().AnyAsync(predicate);
    }

     public async Task DeleteRange(IEnumerable<TEntity> entities)
    {
        dbContext.Set<TEntity>().RemoveRange(entities);
        await Save();
    }

}

