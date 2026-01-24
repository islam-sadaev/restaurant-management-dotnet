namespace Restaurant.Data.Repository
{
    public class CategorieRepository : GenericRepository<Categorie>, ICategorieRepository
    {
        public CategorieRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Categorie>> GetAllCategoriëenPlusProducten()
        {
            return await _context.Categorien.Include(x => x.Producten).ToListAsync();
        }

        public async Task<IEnumerable<Categorie>> GetAllExceptAsync(int id)
        {
            return await _context.Categorien.Where(c => c.Id != id).ToListAsync();
        }

        public async Task<IEnumerable<Categorie>> GetNotDrankenAsync(int id)
        {
            return await _context.Categorien.Where(c => c.TypeId != id).ToListAsync();
        }

        public async Task<IEnumerable<Categorie>> GetAllDrankenAsync(int id)
        {
            return await _context.Categorien.Where(c => c.TypeId == id).ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string naam)
        {
            return await _context.Categorien.AnyAsync(c => c.Naam.ToLower() == naam.ToLower());
        }
    }
}