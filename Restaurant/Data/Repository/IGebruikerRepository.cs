using Restaurant.Models;

namespace Restaurant.Data.Repository
{

    public interface IGebruikerRepository : IGenericRepository<CustomUser>
    {
        Task<CustomUser?> GetUserByIdNoTrackingAsync(string id);
    }
}