
namespace Restaurant.Data.Repository
{
    public interface IMailRepository : IGenericRepository<Mail>
    {
        Task<Mail> GetNieuwsbrief();
    }
}
