using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Configuration.MailService;
using Restaurant.Models;
using Restaurant.ViewModels.Enquete;
using System.Numerics;
using System.Security.Claims;

namespace Restaurant.Controllers
{
    public class KlantController : Controller
    {
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailsender;

        private readonly UserManager<CustomUser> _userManager;

        public KlantController(IUnitOfWork context, IMapper mapper, IEmailSender emailSender, UserManager<CustomUser> usermanager)
        {
            _context = context;
            _mapper = mapper;
            _emailsender = emailSender;
            _userManager = usermanager;
        }

        [Authorize]
        public async Task<ActionResult> TafelReserveren()
        {
            var tijdslotenVanuitDatabase = await _context.TijdslotRepository.GetAllAsync();
            KlantTafelReserverenViewModel model = new KlantTafelReserverenViewModel();
            //model.BeschikbareTijdsloten = new SelectList(tijdslotenVanuitDatabase, "Id", "Tijdslot");

            model.BeschikbareTijdsloten = tijdslotenVanuitDatabase.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> BevestigingsOverzicht(Reservatie reservatie)
        {
            BevestigingsOverzichtViewModel vm = _mapper.Map<BevestigingsOverzichtViewModel>(reservatie);
            var tijdslot = await _context.TijdslotRepository.GetByIdAsync(vm.TijdSlotId);
            vm.Tijdslot = tijdslot.Naam;

            if (vm != null)
            {
                return View(vm);
            }
            else
            {
                return RedirectToAction("TafelReserveren");
            }
        }

        [HttpPost]
        public async Task<IActionResult> BevestigingsOverzicht(BevestigingsOverzichtViewModel vm)
        {
            Reservatie reservatie = _mapper.Map<Reservatie>(vm);

            try
            {
                reservatie.CustomUser = await _userManager.GetUserAsync(User);//zet de costomUser niet meer null

                await _context.ReservatiesRepository.AddAsync(reservatie);
                await _context.SaveChangesAsync();
                var ontvanger = reservatie.CustomUser.Email;
                await Versturen(reservatie, ontvanger);//mail versturen
                return RedirectToAction("ReservatieBedankt"); //moet naar statis website bedantk
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Er is een probleem opgetreden bij het wegschrijven naar de database.");

                return View(vm);
            }
        }

        public IActionResult ReservatieBedankt()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TafelReserveren(KlantTafelReserverenViewModel viewModel)
        {
            if (viewModel.Datum.HasValue && viewModel.Datum.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Datum", "De reservatiedatum kan niet in het verleden liggen.");
            }

            // Herlaad de tijdsloten voor de dropdown
            var tijdslotenVanuitDatabase = await _context.TijdslotRepository.GetAllAsync();
            viewModel.BeschikbareTijdsloten = tijdslotenVanuitDatabase.Select(ts => new SelectListItem
            {
                Value = ts.Id.ToString(),
                Text = ts.Naam
            }).ToList();

            // Stop nu als er fouten aanwezig zijn
            if (ModelState.IsValid)
            {
                Reservatie reservatie = _mapper.Map<Reservatie>(viewModel);
                reservatie.KlantId = _userManager.GetUserId(User);

                var reservaties = await _context.ReservatiesRepository.GetAllReservatieMetTijdSlotAsync();
                ReservatieMetIdListViewModel model = new ReservatieMetIdListViewModel();
                model.Reservaties = _mapper.Map<List<ReservatieMetTijdIdViewModel>>(reservaties); //alle reservaties

                ReservatieMetIdListViewModel resMetTijd = new ReservatieMetIdListViewModel();

                foreach (ReservatieMetTijdIdViewModel item in model.Reservaties)
                {
                    if (item.TijdSlotId == reservatie.TijdSlotId && item.Datum.Value.Date == reservatie.Datum.Value.Date) // kijk of dat de datum en tijdslot correct is
                    {
                        resMetTijd.Reservaties.Add(item);
                    }
                }

                bool plaats = true;
                var waarde = await _context.ParameterRepository.GetByIdAsync(2); //totaal plaatsen in resaurten
                int totaal = int.Parse(waarde.Waarde);
                var resAantal = reservatie.AantalPersonen; //aantal van persoon die nu zijn aan reserverd
                resAantal += (reservatie.AantalPersonen % 2 == 0) ? reservatie.AantalPersonen : reservatie.AantalPersonen + 1;
                if (resAantal == 0)
                {
                    ModelState.AddModelError("", "Het moet meer als 0 zijn");

                    return View(viewModel);
                }

                if (resMetTijd.Reservaties.Count() > 0) // kijk of dat er reservatie zijn
                {
                    int count = resAantal; //wordt totaal reserverd plaatsen
                    foreach (var item in resMetTijd.Reservaties)
                    {
                        if (item.AantalPersonen % 2 == 0)
                        {
                            count += item.AantalPersonen;
                        }
                        else
                        {
                            count += item.AantalPersonen + 1; //oneven getal moet naar boven afgrond worden
                        }
                    }

                    if (resAantal >= totaal) // totaal reserver plaatsen zijn grooter of gelijk dan totaal plaatsen
                    {
                        plaats = false;
                    }
                }
                else //als er noch niks in de lijst zit moet ik ook nakijken of er plek is
                {
                    if (reservatie.AantalPersonen >= totaal)
                    {
                        plaats = false;
                    }
                }

                if (plaats)
                {
                    return RedirectToAction("BevestigingsOverzicht", reservatie);
                }
                else
                {
                    ModelState.AddModelError("", "Er is geen plek op deze moment");

                    return View(viewModel);
                }
            }

            return View(viewModel);
        }

        public async Task Versturen(Reservatie reservatie, string ontvanger)
        {
            reservatie.Tijdslot = await _context.TijdslotRepository.GetByIdAsync(reservatie.TijdSlotId);

            if (ontvanger != null)
            {
                var mail = await _context.MailRepository.GetByIdAsync(2);
                mail.Body = mail.Body.Replace("[VOORNAAM]", reservatie.CustomUser.Voornaam);
                mail.Body = mail.Body.Replace("[DATUM]", reservatie.Datum.ToString());
                mail.Body = mail.Body.Replace("[TIJD]", reservatie.Tijdslot.Naam);
                mail.Body = mail.Body.Replace("[AANTAL]", reservatie.AantalPersonen.ToString());
                _emailsender.SendEmailAsync(ontvanger, mail.Onderwerp, mail.Body);
            }
        }
        public async void ReviewIngeven(int reservatieID)
        {
            VMReviewIngeven viewmodel = new VMReviewIngeven();
          //  viewmodel.ReservatieId = reservatieID;

        }
        [HttpPost]
        public async void ReviewIngeven()
        {

        }
    }
}