using Restaurant.Data.Repository;

namespace Restaurant.Data.UnitOfWork
{
    public interface IUnitOfWork
    {
        IMailRepository MailRepository { get; }

        IBestellingRepository BestellingRepository { get; }

        IReservatiesRepository ReservatiesRepository { get; }
        ITavelLijstRepository TavelLijstRepository { get; }
        IGenericRepository<Tijdslot> TijdslotRepository { get; }
        ITafelRepository TafelRepository { get; }
        IGenericRepository<Parameter> ParameterRepository { get; }
        ICategorieTypeRepository CategorieTypeRepository { get; }
        ICategorieRepository CategorieRepository { get; }
        IProductRepository ProductRepository { get; }
        ILandRepository LandRepository { get; }
        IGebruikerRepository GebruikerRepository { get; }
        IPrijsProductRepository PrijsProductRepository { get; }

        public void SaveChanges();

        public Task SaveChangesAsync();
    }
}