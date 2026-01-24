using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.UnitOfWork;
using Restaurant.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Eigenaar")]
    public class ParameterController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ParameterController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // GET: Toon het formulier
        [HttpGet]
        public async Task<IActionResult> Parameters()
        {
            var dbParams = await _uow.ParameterRepository.GetAllAsync();
            var vm = _mapper.Map<ParameterViewModel>(dbParams);
            return View(vm);
        }

        // POST: Opslaan (Wijzigen)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Parameters(ParameterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // We halen de lijst op om te updaten
                var dbParams = (await _uow.ParameterRepository.GetAllAsync()).ToList();

                // Mapping zorgt voor de update van de waardes
                _mapper.Map(model, dbParams);

                await _uow.SaveChangesAsync();
                TempData["SuccessMessage"] = "Instellingen opgeslagen.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Fout: " + ex.Message);
                return View(model);
            }

            return RedirectToAction(nameof(Parameters));
        }

        // POST: Verwijderen (reset params)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string naam)
        {
            var allParams = await _uow.ParameterRepository.GetAllAsync();
            var param = allParams.FirstOrDefault(p => p.Naam == naam);

            // 1. Zoek de standaardwaarde in je ViewModel Dictionary
            string defaultValue = "";
            if (ParameterViewModel.Defaults.ContainsKey(naam))
            {
                defaultValue = ParameterViewModel.Defaults[naam];
            }

            if (param != null)
            {
                // 2. UPDATE: Overschrijf de waarde met de default in plaats van verwijderen
                param.Waarde = defaultValue;
                _uow.ParameterRepository.Update(param);

                await _uow.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Instelling '{naam}' is gereset naar standaard ('{defaultValue}').";
            }
            else
            {
                // Als de praameter nog niet bestaat in de DB, maak hem dan aan met de default waarde
                var newParam = new Restaurant.Models.Parameter { Naam = naam, Waarde = defaultValue };
                await _uow.ParameterRepository.AddAsync(newParam);
                await _uow.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Instelling '{naam}' is aangemaakt met standaardwaarde.";
            }

            return RedirectToAction(nameof(Parameters));
        }
    }
}