using System.Threading.Tasks;

namespace Restaurant.Data.Repository
{
    public class MailRepository : GenericRepository<Mail>, IMailRepository
    {
        public MailRepository(RestaurantContext context) : base(context) { }

        public async Task<Mail> GetNieuwsbrief()
        {
            List<Mail> list = new List<Mail>();
            list = _context.Mails.ToList();
            return list.Find(m => m.Naam.Contains("Nieuwsbrief:"));          
        }
    }
}
