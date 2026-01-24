namespace Restaurant.Data.Repository
{
    public interface IBestellingRepository : IGenericRepository<Bestelling>
    {
        Task<IList<Bestelling>> SearchBestellingAsync(Expression<Func<Bestelling, bool>>? zoekwaarde);

        Task<IList<Product>> GetAllProducten();

        Task<IList<Bestelling>> GetAllBestellingMetStatus();
        Task<IEnumerable<Bestelling>> GetAllBestellingenVanReservatie(int reservatieId);
    }
}