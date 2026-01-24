namespace Restaurant.Data.Repository
{
    public class TavelLijstRepository : GenericRepository<TafelLijst>, ITavelLijstRepository
    {
        public TavelLijstRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<IList<TafelLijst>> GetAllTafelVanReservatie(int zoekwaarde)
        {
            return await _context.TafelLijsten
                                 .Where(x => x.ReservatieId == zoekwaarde)
                                 .Include(x => x.Tafel)
                                 .ToListAsync();
        }
    }
}