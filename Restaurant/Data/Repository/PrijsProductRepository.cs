
namespace Restaurant.Data.Repository
{
    public class PrijsProductRepository : GenericRepository<PrijsProduct>, IPrijsProductRepository
    {
        public PrijsProductRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<PrijsProduct> GetByIdProductAsync(int id)
        {
            var nu = DateTime.Now;

            return await _context.PrijsProducten
                .Where(p => p.ProductId == id && p.DatumVanaf <= nu)  // enkel verleden
                .OrderByDescending(p => p.DatumVanaf)                // de meest recente datum eerst
                .FirstOrDefaultAsync();
        }

        public async Task<List<PrijsProduct>> GetByIdProductLijstAsync(int id)
        {
            return await _context.PrijsProducten.Where(p => p.ProductId == id).ToListAsync();
        }
    }
}