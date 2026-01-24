using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data.Repository
{
    public class LandRepository : GenericRepository<Land>, ILandRepository
    {
        public LandRepository(RestaurantContext context) : base(context)
        {
        }

        public async Task<List<SelectListItem>> GetActiveLandenSelectListAsync()
        {
            return await _context.Set<Land>()
                .Where(l => l.Actief == true)
                .OrderBy(l => l.Naam)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.Naam
                })
                .ToListAsync();
        }
    }
}
