using FanHubPlus.Data;
using Microsoft.EntityFrameworkCore;

namespace FanHubPlus.Repositories;

// EF Core implementation of the generic repository.
// Scoped lifetime => all repositories in one request share ONE DbContext instance,
// so SaveChangesAsync() writes every pending change atomically (Unit of Work pattern).
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<TEntity> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => _set.AsQueryable(); // tracked query

    public async Task<TEntity?> GetByIdAsync(object id) => await _set.FindAsync(id);

    public async Task<List<TEntity>> ListAsync() => await _set.ToListAsync();

    public async Task AddAsync(TEntity entity) => await _set.AddAsync(entity);

    public void Update(TEntity entity) => _set.Update(entity);   // full update of a detached graph

    public void Remove(TEntity entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
