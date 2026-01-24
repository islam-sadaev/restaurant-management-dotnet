using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Configuration.MailService;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailsender;
        private readonly UserManager<CustomUser> _userManager;

        public HomeController(IUnitOfWork context, IEmailSender emailSender, IMapper mapper, UserManager<CustomUser> userManager)
        {
            _context = context;
            _emailsender = emailSender;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ActionResult> Index()
        {
            var dbParams = await _context.ParameterRepository.GetAllAsync();
            var vm = _mapper.Map<ParameterViewModel>(dbParams);

            return View(vm);
        }

        public async Task<ActionResult> Contact()
        {
            var dbParams = await _context.ParameterRepository.GetAllAsync();
            var vm = _mapper.Map<ParameterViewModel>(dbParams);

            return View(vm);
        }

        public async Task<ActionResult> MenuCategorieType()
        {
            var type = await _context.CategorieTypeRepository.GetAllAsync();
            CategorieTypeListViewModel viewModel = new CategorieTypeListViewModel();
            viewModel.Categorias = _mapper.Map<List<CategorieTypeViewModel>>(type);
            return View(viewModel);
        }

        public async Task<ActionResult> MenuCategorie(int id)
        {
            var categorie = await _context.CategorieRepository.GetAllAsync();
            categorie = categorie.Where(c => c.TypeId == id); //kijk dat ik in de juist categrorie type zit
            CategorieListViewModel viewModel = new CategorieListViewModel();
            viewModel.Categorias = _mapper.Map<List<CategorieViewModel>>(categorie);
            return View(viewModel);
        }

        public async Task<ActionResult> Menu(int id)
        {
            var categorie = await _context.ProductRepository.GetAllAsync();
            categorie = categorie.Where(c => c.CategorieId == id); //kijk dat ik in de juist categrorie zit
            ProductListViewModel viewModel = new ProductListViewModel();
            viewModel.Producten = _mapper.Map<List<ProductViewModel>>(categorie);
            return View(viewModel);
        }

        public async Task<ActionResult> Details(int id)
        {
            var categorie = await _context.ProductRepository.GetByIdAsync(id);
            ProductPrijsViewModel viewModel = new ProductPrijsViewModel();
            var prijs = await _context.PrijsProductRepository.GetByIdProductAsync(id);
            viewModel = _mapper.Map<ProductPrijsViewModel>(categorie);
            viewModel.Prijs = prijs.Prijs;
            return View(viewModel);
        }

        public async Task<ActionResult> VerstuurContact(ParameterContactViewModel vm)
        {
            string ontvanger = "project.CarpMinds@outlook.com";
            string onderwerp = vm.Naam + " " + vm.Email;

            _emailsender.SendEmailAsync(ontvanger, onderwerp, vm.Bericht);

            return RedirectToAction("Contact");
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            // 1) users (identity staat los van UnitOfWork)
            var users = await _userManager.Users.ToListAsync();
            var userCount = users.Count;

            // 2) reservaties via UnitOfWork repository
            var reservaties = await _context.ReservatiesRepository.GetAllAsync();
            var reservatieCount = reservaties?.Count() ?? 0;

            // 3) bestellingen via UnitOfWork
            var bestellingen = await _context.BestellingRepository.GetAllAsync();
            var bestellingCount = bestellingen?.Count() ?? 0;

            // 4) tafels via UnitOfWork
            var tafels = await _context.TafelRepository.GetAllAsync();
            var tafelCount = tafels?.Count() ?? 0;

            // 5) mails via UnitOfWork
            var mails = await _context.MailRepository.GetAllAsync();
            var mailCount = mails?.Count() ?? 0;

            var vm = new AdminDashboardViewModel
            {
                UserCount = userCount,
                ReservatieCount = reservatieCount,
                BestellingCount = bestellingCount,
                TafelCount = tafelCount,
                MailTemplateCount = mailCount
            };

            return View(vm);
        }
    }
}