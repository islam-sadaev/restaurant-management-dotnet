namespace Restaurant.Data.Repository
{
    public interface IReservatiesRepository : IGenericRepository<Reservatie>
    {
        Task<IEnumerable<Reservatie>> GetAllReservatieMetTijdSlotAsync();

        Task<Reservatie> GetByIdReservatieMetTijdSlotAsync(int id);
        Task<IEnumerable<Reservatie>> GetAllReservatieMetTijdSlotVandaagAsync();
        Task<IEnumerable<Reservatie>> Get10MostRecentReviews();
    }
}