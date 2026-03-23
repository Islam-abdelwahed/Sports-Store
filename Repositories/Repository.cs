using Microsoft.EntityFrameworkCore;
using Project.Data;
using System.Linq.Expressions;

namespace Project.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext context;
        private readonly DbSet<T> dbSet;

        public Repository(ApplicationDbContext _context)
        {
            context = _context;
            dbSet = context.Set<T>();
        }
        public async Task AddAsync(T entity)
      =>
            await dbSet.AddAsync(entity);
      

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return dbSet.Where(predicate).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.ToListAsync();
        }

        // Returns IQueryable for advanced filtering/sorting/pagination
        public IQueryable<T> Query()
        {
            return dbSet.AsQueryable();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public async Task SaveAsync()
        {
             await context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }
    }
}