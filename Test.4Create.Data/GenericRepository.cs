using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Test._4Create.Data;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    internal readonly TrialDbContext Context;
    internal readonly DbSet<TEntity> DbSet;

    public GenericRepository(TrialDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>>? filter = null,
                                            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                            string includeProperties = "")
    {
        IQueryable<TEntity> query = DbSet;

        if (filter != null)
        {
            query = query.Where(filter);
        }

        foreach (var includeProperty in includeProperties.Split
                     (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty);
        }

        if (orderBy != null)
        {
            return orderBy(query).ToList();
        }

        return query.ToList();
    }

    public virtual TEntity? GetById(object id)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return DbSet.Find(id);
    }

    public virtual void Insert(TEntity entity)
    {
        DbSet.Add(entity);
    }

    public virtual void Delete(object id)
    {
        var entityToDelete = DbSet.Find(id) ?? throw new InvalidOperationException($"Record with id:{id} couldn't be found into DbSet of type \"{typeof(TEntity).FullName}\"");
        Delete(entityToDelete);
    }

    public virtual void Delete(TEntity entityToDelete)
    {
        if (Context.Entry(entityToDelete).State == EntityState.Detached)
        {
            DbSet.Attach(entityToDelete);
        }

        DbSet.Remove(entityToDelete);
    }

    public virtual void Update(TEntity entityToUpdate)
    {
        DbSet.Attach(entityToUpdate);
        Context.Entry(entityToUpdate).State = EntityState.Modified;
    }
}