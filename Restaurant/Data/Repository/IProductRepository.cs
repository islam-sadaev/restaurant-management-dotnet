namespace Restaurant.Data.Repository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Decimal GetPrijs(int id);

        Task<List<Product>> GetProductsByTypeAsync(int id);

        Task<List<Product>> GetProductsByCategroieAsync(int id);

        Task<List<Product>> GetfoodProductsByTypeAsync();
    }
}