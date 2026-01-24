namespace Restaurant.Data.Repository
{
    public interface IPrijsProductRepository : IGenericRepository<PrijsProduct>
    {
        Task<PrijsProduct> GetByIdProductAsync(int id);

        Task<List<PrijsProduct>> GetByIdProductLijstAsync(int id);
    }
}