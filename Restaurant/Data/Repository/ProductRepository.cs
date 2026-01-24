namespace Restaurant.Data.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(RestaurantContext context) : base(context)
        {
        }

        public Decimal GetPrijs(int id)
        {
            var prijsproducten = _context.PrijsProducten.Where(pp => pp.ProductId == id).Where(pp => pp.DatumVanaf <= DateTime.Now).FirstOrDefault();
            //var huidigPrijsproduct = prijsproducten;
            return prijsproducten.Prijs;
        }

        public async Task<List<Product>> GetProductsByTypeAsync(int id)
        {
            return await _context.Producten.Include(x => x.Categorie).Where(p => p.Categorie.TypeId == id).OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<Product>> GetProductsByCategroieAsync(int id)
        {
            return await _context.Producten.Include(x => x.Categorie).Where(p => p.Categorie.Id == id).OrderByDescending(p => p.Id).ToListAsync();
        }

        public async Task<List<Product>> GetfoodProductsByTypeAsync()
        {
            return await _context.Producten.Include(x => x.Categorie).Where(p => p.Categorie.TypeId != 1).OrderByDescending(p => p.Id).ToListAsync();
        }
    }
}