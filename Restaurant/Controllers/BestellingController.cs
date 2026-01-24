using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Restaurant.Configuration.MailService;
using Restaurant.Models;
using Restaurant.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Kok,Ober,Eigenaar")]
    public class BestellingController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailsender;

        private readonly UserManager<CustomUser> _userManager;

        public BestellingController(IUnitOfWork context, IMapper mapper, UserManager<CustomUser> usermanager, IEmailSender emailSender)
        {
            _context = context;
            _mapper = mapper;
            _userManager = usermanager;

            _emailsender = emailSender;
        }
        
        public async Task<ActionResult<Reservatie>> Afrekenen()
        {
            AfrekenenListViewModel model = new AfrekenenListViewModel();
            
           
            var reservaties = await _context.ReservatiesRepository.GetAllReservatieMetTijdSlotVandaagAsync();
            foreach (Reservatie reservatie in reservaties)
            {// haal alle bestelling op van reservatie
                var bestellingen = await _context.BestellingRepository.GetAllBestellingenVanReservatie(reservatie.Id);
                var prijs = new decimal(0);
                AfrekenenViewModel temp = _mapper.Map<AfrekenenViewModel>(reservatie);
                temp.Bestellingen = new List<BestellingAfrekenenViewModel>();
                foreach (var item in bestellingen)
                {
                    BestellingAfrekenenViewModel bestellingenmetPrijs = new BestellingAfrekenenViewModel();
                    bestellingenmetPrijs.Prijs = _context.ProductRepository.GetPrijs(item.ProductId);
                    var product = await _context.ProductRepository.GetByIdAsync(item.ProductId);
                    bestellingenmetPrijs.Productnaam = product.Naam;
                    temp.Bestellingen.Add(bestellingenmetPrijs);
                    prijs += bestellingenmetPrijs.Prijs;
                }
                temp.TotaalPrijs = prijs;
                model.Reservaties.Add(temp);
            }
           
            
            return View(model);

        }


        [HttpPost]
        public async Task<ActionResult<Reservatie>> AfrekenenBestelling(int id, AfrekenenViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                Reservatie reservatie = await _context.ReservatiesRepository.GetByIdReservatieMetTijdSlotAsync(id);
                reservatie.Bestaald = true;
                _context.ReservatiesRepository.Update(reservatie);
                _context.SaveChanges();

                ReviewMailVersturen(reservatie.CustomUser.Email, reservatie);
                return RedirectToAction("index");

            }
            BevestigingAfrekenenViewModel model = new BevestigingAfrekenenViewModel();
            return View(model);
        }


        public async Task<IActionResult> Bevestiging()
        {
            BevestigingAfrekenenViewModel model = new BevestigingAfrekenenViewModel();
            return View(model);
        }

        public async Task<ActionResult> Create(int id)
        {
            Product product = await _context.ProductRepository.GetByIdAsync(id);
            ProductViewModel newProduct = _mapper.Map<ProductViewModel>(product);

            // Ophalen van huidige lijst uit session
            var selectedProducts = HttpContext.Session.GetObjectFromJson<List<ProductViewModel>>("SelectedProducts")
                                   ?? new List<ProductViewModel>();

            // Toevoegen van nieuw product
            selectedProducts.Add(newProduct);

            // Terug opslaan in session
            HttpContext.Session.SetObjectAsJson("SelectedProducts", selectedProducts);

            // Optioneel: teruggeven naar dezelfde view
            var page = new ProductLijstViewModel { SelectedProducts = selectedProducts };
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductLijstViewModel Bestellingviewmodel)
        {
            if (ModelState.IsValid)
            {
                CustomUser user = await _userManager.FindByNameAsync(User.Identity.Name);

                var reservaties = await _context.ReservatiesRepository.GetAllAsync();

                var vandaag = DateTime.Today;

                var reservatie = reservaties
                    .FirstOrDefault(r =>
                        r.KlantId == user.Id &&
                        r.Datum.Value.Date == vandaag);   // << alleen reservaties van vandaag

                if (reservatie == null)
                {
                    ModelState.AddModelError("", "Je hebt geen reservatie voor vandaag om een bestelling aan toe te voegen.");
                    return View(Bestellingviewmodel);
                }

                foreach (var item in Bestellingviewmodel.Bestellingen)
                {
                    Bestelling bestelling = _mapper.Map<Bestelling>(item);

                    bestelling.ReservatieId = reservatie.Id;
                    bestelling.StatusId = 1;//In behandeling
                    bestelling.TijdstipBestelling = DateTime.Now;
                    await _context.BestellingRepository.AddAsync(bestelling);
                }
            }

            try
            {
                _context.SaveChanges();

                   
                    HttpContext.Session.Remove("SelectedProducts");

                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Er is een probleem opgetreden bij het wegschrijven naar de database.");

                BestellingViewmodel model = new BestellingViewmodel();

                return View(Bestellingviewmodel);
            }
        }

        public async Task<ActionResult> Index()
        {
            List<Bestelling> alleBestellingen = new List<Bestelling>();
            var bestellings = await _context.BestellingRepository.GetAllBestellingMetStatus();
            var vandaag = DateTime.Today;

            if (User.IsInRole("Kok"))
            {
                alleBestellingen = bestellings
                    .Where(b => b.TijdstipBestelling.HasValue && b.TijdstipBestelling.Value.Date == vandaag) //dat het allen bestelling van vandaag zijn
                    .Where(b => b.Product.Categorie != null && b.Product.Categorie.TypeId != 1) //dat het geen draken zijn
                    .Where(b => b.Status.Id != 4) // dat de status niet Onbeschikbaar is
                    .Where(b => b.Status.Id != 3) // dat de status niet Geserveerd is
                    .ToList();
            }
            else if (User.IsInRole("Ober"))
            {
                var bestellingsKok = bestellings //dit is voor kok
                    .Where(b => b.TijdstipBestelling.HasValue && b.TijdstipBestelling.Value.Date == vandaag) //dat het allen bestelling van vandaag zijn
                    .Where(b => b.Status.Id != 4) // dat de status niet Onbeschikbaar is
                    .Where(b => b.Product.Categorie.TypeId != 1) //geen draken
                    .Where(b => b.Status.Id != 1) //dat de status niet In Behandeling is
                    .ToList();

                var bestellingsOber = bestellings //dit is voor ober
                    .Where(b => b.TijdstipBestelling.HasValue && b.TijdstipBestelling.Value.Date == vandaag) //dat het allen bestelling van vandaag zijn
                    .Where(b => b.Status.Id != 4) // dat de status niet Onbeschikbaar is
                    .Where(b => b.Product.Categorie.TypeId == 1) //wel draken
                    .ToList();

                alleBestellingen = bestellingsKok
                    .Concat(bestellingsOber)
                    .ToList();
            }
            BestellingsMetStatusListViewModel model = new BestellingsMetStatusListViewModel();

            model.bestellingMetStatus = _mapper.Map<List<BestellingsMetStatusViewModel>>(alleBestellingen);

            foreach (var besting in model.bestellingMetStatus)
            {
                var tavellijst = await _context.TavelLijstRepository.GetAllTafelVanReservatie(besting.ReservatieId);
                besting.Tafels = tavellijst
                   .Where(t => t.Tafel != null)                 // null check
                   .Select(t => t.Tafel.TafelNummer.ToString()) // maak het een string
                   .ToList();
            }
            model.bestellingMetStatus = model.bestellingMetStatus
                    .OrderBy(vm => vm.Naam)
                    .ThenBy(vm => vm.TijdstipBestelling)
                    .ToList();
            return View(model);
        }

        public async Task<IActionResult> InBehandeling(int id)
        {
            var bestelling = await _context.BestellingRepository.GetByIdAsync(id);
            bestelling.StatusId = 1;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> StaatKlaar(int id)
        {
            var bestelling = await _context.BestellingRepository.GetByIdAsync(id);
            bestelling.StatusId = 2;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Geserveerd(int id)
        {
            var bestelling = await _context.BestellingRepository.GetByIdAsync(id);
            bestelling.StatusId = 3;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Onbeschikbaar(int id)
        {
            var bestelling = await _context.BestellingRepository.GetByIdAsync(id);
            bestelling.StatusId = 4;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public async void ReviewMailVersturen(string ontvanger, Reservatie reservatie)
        {
            if (ontvanger != null)
            {
                var mail = await _context.MailRepository.GetByIdAsync(4);               
                // haal reservatie Id op om mee te geven voor de review pagina
                 var url = Url.Action("ReviewIngeven", "Reservatie", reservatie.Id);
                var klant = await _userManager.FindByIdAsync(reservatie.KlantId);               
                 mail.Body.Replace("[VOORNAAM]", klant.Voornaam);
                 mail.Body.Replace("[Datum]", reservatie.Datum.ToString());
                 mail.Body = '\n' + url;
                 _emailsender.SendEmailAsync(ontvanger, mail.Onderwerp, mail.Body);
                
               
            }
        }
    }
}