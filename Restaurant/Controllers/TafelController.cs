using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Microsoft.IdentityModel.Tokens;
using Restaurant.ViewModels.mails;
using Restaurant.ViewModels.tafels;
namespace Restaurant.Controllers
{
    [Authorize(Roles = "Zaalverantwoordelijke")]
    public class TafelController : Controller
    {

        private readonly ILogger<MailController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _context;
        private readonly UserManager<CustomUser> _userManager;

        public TafelController(ILogger<MailController> logger, IMapper mapper, IUnitOfWork context, UserManager<CustomUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _userManager = userManager;
        }
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            VMTafelPlattegrond vm = new VMTafelPlattegrond();
            vm.TafelLijst = (List<Tafel>)await _context.TafelRepository.GetAllAsync();
            return View(vm);
        }
        public async Task<IActionResult> TafelWijzigen(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var tafel = await _context.TafelRepository.GetByIdAsync(id);
            VMTafelEdit viewModel = new VMTafelEdit();
            viewModel = _mapper.Map<VMTafelEdit>(tafel);
            if (tafel == null)
            {
                return RedirectToAction("Index");
            }
            if(tafel.TafelLijsten.IsNullOrEmpty() == false)
            {
                TempData["alertMessage"] = "Deze tafel heeft lopende bestelling, Wijzig op risico.";
            }
          
            return View(viewModel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TafelWijzigen(int id, Tafel tafel)
        {


            if (id != tafel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TafelRepository.Update(tafel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _context.MailRepository.GetByIdAsync(tafel.Id) != null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(tafel);
        }
        [AllowAnonymous]
        public async Task<IActionResult> TafelToewijzen(TafelLijstToewijzen viewModel)
        {
            var reservaties = await _context.ReservatiesRepository.GetAllReservatieMetTijdSlotVandaagAsync();
            if (reservaties == null)
            {
                return RedirectToAction("Index");
            }
            var tafels = await _context.TafelRepository.GetTafels();
            if (tafels == null)
            {
                return RedirectToAction("Index");
            }
            viewModel.BeschickbareTafels = _mapper.Map<List<Tafel>>(tafels);
            viewModel.Reservaties = _mapper.Map<List<ReservatieTafelsToewijzen>>(reservaties);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddTafelsToList(IList<Tafel> tafels, TafelLijstToewijzen viewmodel) 
        {
            CustomUser user = await _userManager.FindByNameAsync(User.Identity.Name);
            var i = 0;
            
            foreach (Tafel t in tafels)
            {
                TafelLijst tafelLijst = new TafelLijst();
                tafelLijst.ReservatieId = user.Reservaties.FirstOrDefault(r => r.Datum == DateTime.Today).Id;
                tafelLijst.TafelId = tafels[i].Id;
                i++;
            }
            
            return View(viewmodel);
        }


        public async Task<IActionResult> TafelAanmaken()
        {
            VMTafelAanmaken viewModel = new VMTafelAanmaken();
          
            return View(viewModel);

        }
        [HttpPost]
        public async Task<IActionResult> TafelAanmaken(VMTafelAanmaken viewModel)
        {
            if (ModelState.IsValid)
            {
                Tafel tafel = new Tafel();
                tafel = _mapper.Map<Tafel>(viewModel);
                return RedirectToAction("TafelBevestigingoverzicht",tafel);
            }
            return View(viewModel);
        }
        public async  Task<IActionResult> TafelBevestigingoverzicht(Tafel tafel)
        {
            VMTafelBevestigen viewmodel = new VMTafelBevestigen();
            viewmodel.Tafel = tafel;

           return  View(viewmodel);
        }
        [HttpPost]
        public async Task<IActionResult> TafelBevestigingoverzicht(VMTafelBevestigen viewmodel)
        {
            if (ModelState.IsValid)
            {
                Tafel tafel = viewmodel.Tafel;
                await _context.TafelRepository.AddAsync(tafel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewmodel);
        }
        public async Task<IActionResult> Delete(int id)
        {
            Tafel tafel = await _context.TafelRepository.GetByIdAsync(id);
            if(tafel != null)
            {
                _context.TafelRepository.Delete(tafel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
