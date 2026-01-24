using Restaurant.ViewModels.tafels;

namespace Restaurant.Data.Repository
{
    public class BestellingRepository : GenericRepository<Bestelling>, IBestellingRepository
    {
        public BestellingRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IList<Bestelling>> SearchBestellingAsync(Expression<Func<Bestelling, bool>>? zoekwaarde)
        {
            return await _context.Bestellingen
                        .Where(zoekwaarde)
                        .OrderByDescending(x => x.Reservatie)
                        .ToListAsync();
        }

        public async Task<IList<Product>> GetAllProducten()
        {
            return await _context.Producten.ToListAsync();
        }

        public async Task<IList<Bestelling>> GetAllBestellingMetStatus()
        {
            return await _context.Bestellingen.Include(x => x.Status).Include(x => x.Product).ThenInclude(x => x.Categorie).Include(x => x.Reservatie).ThenInclude(x => x.CustomUser).ToListAsync();
        }
        public  async Task<IEnumerable<Bestelling>> GetAllBestellingenVanReservatie(int reservatieId)
        {
           var bestellingen = await _context.Bestellingen.Where(b => b.ReservatieId == reservatieId).ToListAsync();
            return bestellingen;//await _context.Bestellingen.Include(x => x.Status).Include(x => x.Product).ThenInclude(x => x.Categorie).Include(x => x.Reservatie).ThenInclude(x => x.CustomUser).ToListAsync();
        }      

        public override async Task<IEnumerable<Bestelling>> GetAllAsync()
        {
            return await _context.Bestellingen
                .Include(b => b.Product)
                    .ThenInclude(p => p.PrijsProducten)
                .Include(b => b.Reservatie)
                    .ThenInclude(r => r.CustomUser)
                .Include(b => b.Status)
                .ToListAsync();
        }

        public override async Task<Bestelling?> GetByIdAsync(int id)
        {
            return await _context.Bestellingen
                .Include(b => b.Product)
                    .ThenInclude(p => p.PrijsProducten)
                .Include(b => b.Reservatie)
                    .ThenInclude(r => r.CustomUser)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
}