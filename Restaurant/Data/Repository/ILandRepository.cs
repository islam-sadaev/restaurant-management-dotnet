using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Models;

namespace Restaurant.Data.Repository
{
    public interface ILandRepository : IGenericRepository<Land>
    {
        Task<List<SelectListItem>> GetActiveLandenSelectListAsync();
    }
}