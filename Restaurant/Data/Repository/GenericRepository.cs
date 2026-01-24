using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore; // Deze is nodig voor ToListAsync en Include

namespace Restaurant.Data.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly RestaurantContext _context;

        public GenericRepository(RestaurantContext context)
        {
            _context = context;
        }

        // 'virtual' toegevoegd zodat je deze kunt overriden
        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        // AANGEPAST:
        // 1. 'virtual' toegevoegd.
        // 2. '<T>' weggehaald en 'T id' veranderd naar 'int id'.
        // Dit zorgt dat het matcht met jouw override in ReservatieRepository.
        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task AddAsync(TEntity entity)
        {
            try
            {
                await _context.Set<TEntity>().AddAsync(entity);
            }
            catch (Exception e)
            {
                throw new Exception("" + e.Message);
            }
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public async Task<IList<TEntity>> Find(Expression<Func<TEntity, bool>>? voorwaarden,
            params Expression<Func<TEntity, object>>[]? includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (includes != null)
            {
                foreach (var item in includes)
                {
                    query = query.Include(item);
                }
            }

            if (voorwaarden != null)
            {
                query = query.Where(voorwaarden);
            }

            return await query.ToListAsync();
        }
    }
}