namespace Restaurant.Data.Repository
{
    public class TafelRepository : GenericRepository<Tafel>, ITafelRepository
    {
        public TafelRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IList<Tafel>> GetTafels()
        {
            return await _context.Tafels.Where(t => t.Actief == true).ToListAsync();
        }
    }
}