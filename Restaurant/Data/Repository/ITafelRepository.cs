namespace Restaurant.Data.Repository
{
    public interface ITafelRepository : IGenericRepository<Tafel>
    {
        Task<IList<Tafel>> GetTafels();
    }
}