using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Configuration.MailService;
using Restaurant.Models;
using Restaurant.ViewModels.mails;
using System.Diagnostics;
using System.Numerics;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Eigenaar")]
    public class MailController : Controller
    {
        private readonly ILogger<MailController> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _context;
        private readonly IEmailSender _emailsender;
        private readonly UserManager<CustomUser> _userManager;
        public MailController(ILogger<MailController> logger, IMapper mapper, IUnitOfWork context, IEmailSender emailSender, UserManager<CustomUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
            _emailsender = emailSender;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var mails = await _context.MailRepository.GetAllAsync();

            VMMailTemplateList viewModel = new VMMailTemplateList();
            viewModel.MailTemplates = mails.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Naam
            }).ToList();
            return View(viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(VMMailTemplateList viewModel)
        {
            var id = viewModel.TemplateId;
            if (id == -1)
            {
                return RedirectToAction("Index");
            }

            var mail = await _context.MailRepository.GetByIdAsync(id);
            if (mail == null)
            {
                return RedirectToAction("Index");
            }
            return View(mail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Mail mail)
        {
            if (id != mail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.MailRepository.Update(mail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _context.MailRepository.GetByIdAsync(mail.Id) != null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                Versturen("project.CarpMinds@outlook.com", mail);
                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        public async Task<IActionResult> Nieuwsbrief()
        {
            VMNiewsbrief viewModel = new VMNiewsbrief();
            //haal klant emails
            viewModel.Ontvangers = new();
            viewModel.Ontvangers.Add("project.CarpMinds@outlook.com");
            IList<CustomUser> klanten = await _userManager.GetUsersInRoleAsync("Klant");
            foreach (CustomUser klant in klanten)
            {
                if (klant.Actief == true)
                {
                    viewModel.Ontvangers.Add(klant.Email);
                }
            }

            Mail template = await _context.MailRepository.GetNieuwsbrief();
            if (template != null)
            {
                //Mapper geeft steeds errors daarom opdeze manier |viewModel = _mapper.Map<VMNiewsbrief>(template);
                viewModel.Naam = template.Naam.Replace("Nieuwsbrief: ", "");
                viewModel.Onderwerp = template.Onderwerp;
                viewModel.Body = template.Body;
                viewModel.Id = template.Id;
                NieuwsbriefTemplateVerwijderen(template);
                await _context.SaveChangesAsync();
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Nieuwsbrief(VMNiewsbrief viewModel)
        {
            Mail nieuwsbrief = new Mail();
            Mail template = await _context.MailRepository.GetByIdAsync(viewModel.Id);
            if (ModelState.IsValid)
            {
                nieuwsbrief.Naam = "Nieuwsbrief: " + viewModel.Naam;
                nieuwsbrief.Onderwerp = viewModel.Onderwerp;
                nieuwsbrief.Body = viewModel.Body;
            }
            try
            {
                string mailLog = NiewsbriefVersturen(viewModel.Ontvangers, nieuwsbrief);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                //Error message
                try
                {
                    {
                        await _context.MailRepository.AddAsync(nieuwsbrief);
                        await _context.SaveChangesAsync();
                    }
                    TempData["alertMessage"] = "Er ging iets mis met het versturen van de nieuwsbrief.";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    return RedirectToAction("Index");
                }
            }
        }

        public async Task<IActionResult> Aanmaken()
        {
            VMMailTemplateAanmaken viewModel = new VMMailTemplateAanmaken();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Aanmaken(VMMailTemplateAanmaken viewModel)
        {
            if (ModelState.IsValid)
            {
                Mail mail = new();

                mail.Naam = viewModel.Naam;
                mail.Body = viewModel.Body;
                mail.Onderwerp = viewModel.Onderwerp;
                await _context.MailRepository.AddAsync(mail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public void Versturen(string ontvanger, Mail mail)
        {
            if (ontvanger != null)
            {
                _emailsender.SendEmailAsync(ontvanger, mail.Onderwerp, mail.Body);
            }
        }

        public string NiewsbriefVersturen(List<string> ontvangers, Mail mail)
        {
            string mailLogInfo = "";
            foreach (string ontvanger in ontvangers)
            {
                mailLogInfo = ontvanger;
                try
                {
                    Versturen(ontvanger, mail);
                    mailLogInfo += ": Succes" + '\n';
                }
                catch
                {
                    mailLogInfo += ": Failed" + '\n';
                }
            }
            Mail mailLog = new Mail();
            mailLog.Naam = "NieuwsBrief_Log  " + DateTime.Now.Date.ToString();
            mailLog.Onderwerp = "NieuwsBrief_Log  " + DateTime.Now.Date.ToString();
            mailLog.Body = mailLogInfo;
            Versturen("project.CarpMinds@outlook.com", mailLog);
            return mailLogInfo;
        }

        [HttpGet]
        public async Task<IActionResult> LogNieuwsbrief(string log)
        {
            VMLog viewModel = new VMLog();
            viewModel.Logs = log;

            return View(viewModel);
        }

        public void NieuwsbriefTemplateVerwijderen(Mail template)
        {
            if (template != null)
            {
                _context.MailRepository.Delete(template);
            }
        }
    }
}