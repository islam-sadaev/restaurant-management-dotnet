using Microsoft.EntityFrameworkCore;

namespace Restaurant.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly RestaurantContext _context;
        private IMailRepository mailRepository;
        private IBestellingRepository bestellingRepository;
        private ITavelLijstRepository tavelLijstRepository;
        private IReservatiesRepository reservatiesRepository;
        private IGenericRepository<Tijdslot> tijdSlotRepository;
        private IProductRepository productRepository;
        private ITafelRepository tafelRepository;
        private ICategorieRepository categorieRepository;
        private IGenericRepository<Parameter> parameterRepository;
        private ICategorieTypeRepository categorieTypeRepository;

        private IPrijsProductRepository _prijsProductRepository;

        private ILandRepository _landRepository;
        private IGebruikerRepository _gebruikerRepository;

        public UnitOfWork(RestaurantContext context)
        {
            _context = context;
        }

        public IBestellingRepository BestellingRepository
        {
            get
            {
                return bestellingRepository ??= new BestellingRepository(_context);
            }
        }

        public ITavelLijstRepository TavelLijstRepository
        {
            get
            {
                return tavelLijstRepository ??= new TavelLijstRepository(_context);
            }
        }

        public IPrijsProductRepository PrijsProductRepository
        {
            get
            {
                return _prijsProductRepository ??= new PrijsProductRepository(_context);
            }
        }

        public IMailRepository MailRepository
        {
            get
            {
                return mailRepository ??= new MailRepository(_context);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IReservatiesRepository ReservatiesRepository
        {
            get
            {
                return reservatiesRepository ??= new ReservatieRepository(_context);
            }
        }

        public IGenericRepository<Tijdslot> TijdslotRepository
        {
            get
            {
                return tijdSlotRepository ??= new GenericRepository<Tijdslot>(_context);
            }
        }

        public ITafelRepository TafelRepository
        {
            get
            {
                return tafelRepository ??= new TafelRepository(_context);
            }
        }

        public IGenericRepository<Parameter> ParameterRepository
        {
            get
            {
                return parameterRepository ??= new GenericRepository<Parameter>(_context);
            }
        }

        public ICategorieTypeRepository CategorieTypeRepository
        {
            get
            {
                return categorieTypeRepository ??= new CategorieTypeRepository(_context);
            }
        }

        public ICategorieRepository CategorieRepository
        {
            get
            {
                return categorieRepository ??= new CategorieRepository(_context);
            }
        }

        public IProductRepository ProductRepository
        {
            get
            {
                return productRepository ??= new ProductRepository(_context);
            }
        }

        public ILandRepository LandRepository
        {
            get
            {
                return _landRepository ??= new LandRepository(_context);
            }
        }

        public IGebruikerRepository GebruikerRepository
        {
            get
            {
                return _gebruikerRepository ??= new GebruikerRepository(_context);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}