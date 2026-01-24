namespace Restaurant.Data.Repository
{
    public interface ICategorieTypeRepository : IGenericRepository<CategorieType>
    {
        Task<IEnumerable<CategorieType>> GetAllExceptAsync(int id);
    }
}