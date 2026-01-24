namespace Restaurant.Data.Repository
{
    public interface ITavelLijstRepository : IGenericRepository<TafelLijst>
    {
        Task<IList<TafelLijst>> GetAllTafelVanReservatie(int zoekwaarde);
    }
}