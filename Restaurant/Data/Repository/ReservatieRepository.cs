namespace Restaurant.Data.Repository
{
    public class ReservatieRepository : GenericRepository<Reservatie>, IReservatiesRepository
    {
        public ReservatieRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reservatie>> GetAllReservatieMetTijdSlotAsync()
        {
            return await _context.Reservaties.Include(x => x.Tijdslot).Include(x => x.CustomUser).ToListAsync();
        }

        public async Task<Reservatie?> GetByIdReservatieMetTijdSlotAsync(int id)
        {
            return await _context.Reservaties.Include(x => x.Tijdslot).Include(x => x.CustomUser).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Reservatie>> GetAllReservatieMetTijdSlotVandaagAsync()
        {
            return await _context.Reservaties.Include(x => x.Tijdslot).Where(x => x.Datum == DateTime.Today).Include(x => x.CustomUser).ToListAsync();
        }

        public async Task<IEnumerable<Reservatie>> Get10MostRecentReviews()
        {
             IEnumerable<Reservatie> var = _context.Reservaties.Where(r => r.EvaluatieOpmerkingen != null);
            if(var.Count() > 10)
            {
               var = var.ToList().GetRange(var.Count(), -10);
            }
            return var;


        }

        public override async Task<IEnumerable<Reservatie>> GetAllAsync()
        {
            return await _context.Reservaties
                .Include(r => r.CustomUser) // EF gebruikt KlantId automatisch via OnModelCreating
                .Include(r => r.Tijdslot)
                .Include(r => r.Bestellingen)
                .ToListAsync();
        }

        public override async Task<Reservatie?> GetByIdAsync(int id)
        {
            return await _context.Reservaties
                .Include(r => r.CustomUser)
                .Include(r => r.Tijdslot)
                .Include(r => r.Bestellingen)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

    }
}