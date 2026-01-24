namespace Restaurant.Data.Repository
{
    public interface ICategorieRepository : IGenericRepository<Categorie>
    {
        Task<IEnumerable<Categorie>> GetAllCategoriëenPlusProducten();

        Task<IEnumerable<Categorie>> GetAllExceptAsync(int id);

        Task<IEnumerable<Categorie>> GetNotDrankenAsync(int id);

        Task<IEnumerable<Categorie>> GetAllDrankenAsync(int id);

        Task<bool> ExistsByNameAsync(string naam);
    }
}