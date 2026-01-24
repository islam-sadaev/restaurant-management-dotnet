namespace Restaurant.Data.Repository
{
    public class CategorieTypeRepository : GenericRepository<CategorieType>, ICategorieTypeRepository
    {
        public CategorieTypeRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CategorieType>> GetAllExceptAsync(int id)
        {
            return await _context.Types.Where(c => c.Id != id).ToListAsync();
        }
    }
}