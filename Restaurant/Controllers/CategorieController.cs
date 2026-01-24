using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class CategorieController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;

        public CategorieController(IUnitOfWork context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ActionResult> Index()
        {
            var categorieen = await _context.CategorieRepository.GetAllAsync();
            CategorieenListViewModel model = new CategorieenListViewModel();

            model.Categorieen = _mapper.Map<List<CategorieenViewModel>>(categorieen);

            return View(model);
        }

        public async Task<ActionResult> EditCategorieen(int id)
        {
            var categorie = await _context.CategorieRepository.GetByIdAsync(id);

            if (categorie == null)
            {
                return NotFound();
            }
            CategorieenViewModel model = new CategorieenViewModel();
            model = _mapper.Map<CategorieenViewModel>(categorie);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditCategorieen(CategorieenViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var categorie = await _context.CategorieRepository.GetByIdAsync(model.Id);
            if (categorie == null)
            {
                return NotFound();
            }

            categorie.Naam = model.Naam;
            categorie.Actief = model.Actief;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> AddCategorieen()
        {
            CategorieenCreateViewModel model = new CategorieenCreateViewModel();
            // Load categorieenType into the dropdown
            var type = await _context.CategorieTypeRepository.GetAllAsync();
            model.CategorienType = type.Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = k.Naam
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCategorieen(CategorieenCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Dropdown opnieuw laden!
                var types = await _context.CategorieTypeRepository.GetAllAsync();

                model.CategorienType = types.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Naam
                }).ToList();

                return View(model);
            }

            // Duplicate check: bestaat er al een categorie met dezelfde naam?
            var bestaat = await _context.CategorieRepository.ExistsByNameAsync(model.Naam);
            if (bestaat)
            {
                ModelState.AddModelError(nameof(model.Naam), "Er bestaat al een categorie met deze naam.");

                // Dropdown opnieuw vullen
                var types = await _context.CategorieTypeRepository.GetAllAsync();
                model.CategorienType = types.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Naam
                }).ToList();

                return View(model);
            }

            Categorie categorie = _mapper.Map<Categorie>(model);

            await _context.CategorieRepository.AddAsync(categorie);
            _context.SaveChanges();

            // Foto opslaan als er een file is geüpload
            if (model.Foto != null && model.Foto.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/Categorie");

                // Folder aanmaken als deze nog niet bestaat
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{categorie.Id}{Path.GetExtension(model.Foto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Foto.CopyToAsync(stream);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteCategorieen(int id)
        {
            var categorie = await _context.CategorieRepository.GetByIdAsync(id);
            var gerechten = await _context.ProductRepository.GetProductsByCategroieAsync(id);

            if (!gerechten.Any())
            {
                // Geen gerechten → direct verwijderen
                _context.CategorieRepository.Delete(categorie);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Alle andere categorieën voor dropdown
            var andereCategorieen = await _context.CategorieRepository.GetAllExceptAsync(id);

            CategorieReassignViewModel vm = new CategorieReassignViewModel();
            vm = _mapper.Map<CategorieReassignViewModel>(categorie);

            vm.Gerechten = gerechten.Select(g => new GerechtReassignItem
            {
                Id = g.Id,
                Naam = g.Naam
            }).ToList();
            vm.Categorieen = andereCategorieen.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Naam
            }).ToList();

            return View("DeleteCategorieen", vm);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCategorieen(CategorieReassignViewModel model)
        {
            // Loop door alle gerechten die verplaatst moeten worden
            foreach (var gerecht in model.Gerechten)
            {
                // Haal het gerecht uit de database
                var entity = await _context.ProductRepository.GetByIdAsync(gerecht.Id);

                // Update naar de categorie die de gebruiker gekozen heeft
                entity.CategorieId = gerecht.NieuweCategorieId;
            }

            // Verwijder daarna de categorie nu hij leeg is
            var categorie = await _context.CategorieRepository.GetByIdAsync(model.Id);
            _context.CategorieRepository.Delete(categorie);

            // wijzigingen opslaan
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}