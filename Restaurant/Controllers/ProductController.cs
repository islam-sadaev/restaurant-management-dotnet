namespace Restaurant.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;

        public ProductController(IUnitOfWork context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IActionResult> DrankOverzicht()
        {
            var dranken = await _context.ProductRepository.GetProductsByTypeAsync(1);
            DrankenLijstViewModel drankenLijst = new DrankenLijstViewModel();
            drankenLijst.dranken = _mapper.Map<List<DrankenViewModel>>(dranken);
            foreach (var drank in drankenLijst.dranken)
            {
                var productPrijs = await _context.PrijsProductRepository.GetByIdProductAsync(drank.Id);
                drank.prijs = productPrijs.Prijs;
            }
            return View(drankenLijst);
        }

        public async Task<IActionResult> CreateDrank()
        {
            var categorie = await _context.CategorieRepository.GetAllDrankenAsync(1);
            DrankenViewModel viewModel = new DrankenViewModel();
            viewModel.categories = categorie.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDrank(DrankenViewModel drank)
        {
            if (!ModelState.IsValid)
                return View(drank);

            // Product aanmaken
            var drink = _mapper.Map<Product>(drank);
            await _context.ProductRepository.AddAsync(drink);
            await _context.SaveChangesAsync(); // <-- Id wordt hier gegenereerd

            // Prijs koppelen
            var prijs = _mapper.Map<PrijsProduct>(drank);
            prijs.ProductId = drink.Id;
            prijs.DatumVanaf = DateTime.Today;

            await _context.PrijsProductRepository.AddAsync(prijs);
            await _context.SaveChangesAsync();

            // Foto opslaan
            if (drank.Foto != null && drank.Foto.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/Menu"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{drink.Id}{Path.GetExtension(drank.Foto.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await drank.Foto.CopyToAsync(stream);
            }

            return RedirectToAction("DrankOverzicht", "Product");
        }

        public async Task<IActionResult> DrankEdit(int id)
        {
            var drank = await _context.ProductRepository.GetByIdAsync(id);
            DrankenEditViewModel viewModel = new DrankenEditViewModel();
            viewModel = _mapper.Map<DrankenEditViewModel>(drank);
            var price = _context.ProductRepository.GetPrijs(drank.Id);

            var categorie = await _context.CategorieRepository.GetAllDrankenAsync(1);
            viewModel.categories = categorie.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            viewModel.prijs = price;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DrankEdit(DrankenEditViewModel vm)
        {
            var drank = await _context.ProductRepository.GetByIdAsync(vm.Id);
            if (drank == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.categories = (await _context.CategorieRepository.GetAllDrankenAsync(1))
                                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Naam }).ToList();
                return View(vm);
            }

            try
            {
                _mapper.Map(vm, drank); // map naar bestaand object
                _context.ProductRepository.Update(drank);

                // Eventueel: prijs updaten als vm.prijs != huidige prijs
                var prijs = await _context.PrijsProductRepository.GetByIdProductAsync(drank.Id);
                if (prijs != null)
                {
                    prijs.Prijs = vm.prijs;
                    _context.PrijsProductRepository.Update(prijs);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("DrankOverzicht", "Product");
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DrankDelete(int id)
        {
            // Haal het product op uit de database
            var drank = await _context.ProductRepository.GetByIdAsync(id);
            if (drank == null)
            {
                return NotFound();
            }

            // Verwijder eerst alle gerelateerde PrijsProduct records
            var prijzen = await _context.PrijsProductRepository.GetByIdProductLijstAsync(id);
            if (prijzen.Any())
            {
                foreach (var prijs in prijzen)
                {
                    _context.PrijsProductRepository.Delete(prijs);
                }
            }

            // Verwijder daarna het product zelf
            _context.ProductRepository.Delete(drank);

            // Sla de wijzigingen op
            await _context.SaveChangesAsync();

            // Blijf op dezelfde pagina
            return RedirectToAction("DrankOverzicht", "Product");
        }

        public async Task<IActionResult> EtenOverzicht()
        {
            var eten = await _context.ProductRepository.GetfoodProductsByTypeAsync();
            EtenLijstViewModel etenLijst = new EtenLijstViewModel();
            etenLijst.eten = _mapper.Map<List<EtenOverzichtItem>>(eten);
            foreach (var gerecht in etenLijst.eten)
            {
                var productPrijs = await _context.PrijsProductRepository.GetByIdProductAsync(gerecht.Id);
                gerecht.Prijs = productPrijs.Prijs;
            }
            return View(etenLijst);
        }

        public async Task<IActionResult> CreateEten()
        {
            var categorie = await _context.CategorieRepository.GetNotDrankenAsync(1);
            EtenViewModel viewModel = new EtenViewModel();
            viewModel.categories = categorie.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEten(EtenViewModel eten)
        {
            if (!ModelState.IsValid)
                return View(eten);

            // Product aanmaken
            var gerecht = _mapper.Map<Product>(eten);
            await _context.ProductRepository.AddAsync(gerecht);
            await _context.SaveChangesAsync(); // Id is nu beschikbaar

            // Prijs koppelen
            var prijs = _mapper.Map<PrijsProduct>(eten);
            prijs.ProductId = gerecht.Id;
            prijs.DatumVanaf = DateTime.Today;

            await _context.PrijsProductRepository.AddAsync(prijs);
            await _context.SaveChangesAsync();

            // 📸 Foto opslaan
            if (eten.Foto != null && eten.Foto.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "Menu"
                );

                // Folder aanmaken indien nodig
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileExtension = Path.GetExtension(eten.Foto.FileName);
                var fileName = $"{gerecht.Id}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await eten.Foto.CopyToAsync(stream);
            }

            return RedirectToAction("EtenOverzicht", "Product");
        }

        public async Task<IActionResult> EtenEdit(int id)
        {
            var eten = await _context.ProductRepository.GetByIdAsync(id);
            EtenEditViewModel viewModel = new EtenEditViewModel();
            viewModel = _mapper.Map<EtenEditViewModel>(eten);
            var price = _context.ProductRepository.GetPrijs(eten.Id);

            var categorie = await _context.CategorieRepository.GetNotDrankenAsync(1);

            viewModel.categories = categorie.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            viewModel.prijs = price;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtenEdit(EtenEditViewModel vm)
        {
            var eten = await _context.ProductRepository.GetByIdAsync(vm.Id);
            if (eten == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.categories = (await _context.CategorieRepository.GetNotDrankenAsync(1))
                      .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Naam }).ToList();
                return View(vm);
            }

            try
            {
                _mapper.Map(vm, eten); // map naar bestaand object
                _context.ProductRepository.Update(eten);

                // Eventueel: prijs updaten als vm.prijs != huidige prijs
                var prijs = await _context.PrijsProductRepository.GetByIdProductAsync(eten.Id);
                if (prijs != null)
                {
                    prijs.Prijs = vm.prijs;
                    _context.PrijsProductRepository.Update(prijs);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("EtenOverzicht", "Product");
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EtenDelete(int id)
        {
            // Haal het product op uit de database
            var eten = await _context.ProductRepository.GetByIdAsync(id);
            if (eten == null)
            {
                return NotFound();
            }

            // Verwijder eerst alle gerelateerde PrijsProduct records
            var prijzen = await _context.PrijsProductRepository.GetByIdProductLijstAsync(id);
            if (prijzen.Any())
            {
                foreach (var prijs in prijzen)
                {
                    _context.PrijsProductRepository.Delete(prijs);
                }
            }

            // Verwijder daarna het product zelf
            _context.ProductRepository.Delete(eten);

            // Sla de wijzigingen op
            await _context.SaveChangesAsync();

            // Blijf op dezelfde pagina
            return RedirectToAction("EtenOverzicht", "Product");
        }
    }
}