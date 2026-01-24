using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data.Repository
{
    public class GebruikerRepository : GenericRepository<CustomUser>, IGebruikerRepository
    {
        public GebruikerRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<CustomUser?> GetUserByIdNoTrackingAsync(string id)
        {

            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}