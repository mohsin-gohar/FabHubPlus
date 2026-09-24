using FanHubPlus.Models.Entities;

namespace FanHubPlus.Repositories;

// Generic Repository: one small, reusable data-access contract for EVERY entity.
// Controllers never touch ApplicationDbContext directly -> testable + swappable.
public interface IRepository<TEntity> where TEntity : class
{
    // Compose LINQ (Include / Where / OrderBy) without exposing the whole DbContext
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(object id);
    Task<List<TEntity>> ListAsync();
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    // Flushes ALL pending changes of the shared scoped DbContext (= Unit of Work)
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
