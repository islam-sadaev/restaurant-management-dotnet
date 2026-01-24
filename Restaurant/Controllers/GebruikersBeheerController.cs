using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Configuration.MailService;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Eigenaar")]
    public class GebruikersBeheerController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly RestaurantContext _context;

        public GebruikersBeheerController(
            UserManager<CustomUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            RestaurantContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Naam = $"{user.Voornaam} {user.Achternaam}",
                    Email = user.Email,
                    Rollen = roles.ToList(),
                    Actief = user.Actief
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "Dit e-mailadres bestaat al.");
                return View(model);
            }

            var standaardLand = await _context.Landen.FirstOrDefaultAsync();
            int landId = standaardLand?.Id ?? 1;

            var user = new CustomUser
            {
                UserName = model.Email,
                Email = model.Email,
                Voornaam = model.Voornaam,
                Achternaam = model.Achternaam,
                Actief = true,
                LandId = landId
            };

            var result = await _userManager.CreateAsync(user, model.TijdelijkWachtwoord);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync(model.Rol))
                await _roleManager.CreateAsync(new IdentityRole(model.Rol));

            await _userManager.AddToRoleAsync(user, model.Rol);

            // FIX 1: User opnieuw ophalen voor verse SecurityStamp
            user = await _userManager.FindByIdAsync(user.Id);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // FIX 2: Token encoden voor URL veiligheid
            var encodedToken = System.Net.WebUtility.UrlEncode(token);

            var link = Url.Action(
                "ResetPassword",
                "Gebruiker",
                new { email = user.Email, token = encodedToken },
                Request.Scheme);

             _emailSender.SendEmailAsync(
                user.Email,
                "Stel je wachtwoord in",
                $"Klik op onderstaande link om je wachtwoord in te stellen:<br><a href=\"{link}\">Wachtwoord instellen</a>"
            );

            TempData["Message"] = "Gebruiker succesvol aangemaakt en resetlink verzonden.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction(nameof(Index));

            var roles = await _userManager.GetRolesAsync(user);

            return View(new UserEditViewModel
            {
                Id = user.Id,
                Voornaam = user.Voornaam,
                Achternaam = user.Achternaam,
                Email = user.Email,
                Rol = roles.FirstOrDefault() ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return RedirectToAction(nameof(Index));

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != user.Id)
            {
                ModelState.AddModelError("Email", "Dit e-mailadres is al in gebruik.");
                return View(model);
            }

            user.Voornaam = model.Voornaam;
            user.Achternaam = model.Achternaam;
            user.Email = model.Email;
            user.UserName = model.Email;

            await _userManager.UpdateAsync(user);

            var huidigeRollen = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, huidigeRollen);

            if (!await _roleManager.RoleExistsAsync(model.Rol))
                await _roleManager.CreateAsync(new IdentityRole(model.Rol));

            await _userManager.AddToRoleAsync(user, model.Rol);

            TempData["Message"] = "Gebruiker succesvol bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return RedirectToAction(nameof(Index));

            var owners = await _userManager.GetUsersInRoleAsync("Eigenaar");
            if (await _userManager.IsInRoleAsync(user, "Eigenaar") && owners.Count == 1)
            {
                TempData["Error"] = "Het laatste eigenaar-account kan niet verwijderd worden.";
                return RedirectToAction(nameof(Index));
            }

            var rollen = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, rollen);

            user.Voornaam = "<<Verwijderd>>";
            user.Achternaam = "<<Verwijderd>>";
            user.Email = $"verwijderd_{user.Id}@removed.local";
            user.NormalizedEmail = user.Email.ToUpper();
            user.UserName = user.Email;
            user.NormalizedUserName = user.Email.ToUpper();
            user.Adres = "<<Verwijderd>>";
            user.Gemeente = "<<Verwijderd>>";
            user.Actief = false;

            await _userManager.UpdateAsync(user);

            TempData["Message"] = "Gebruiker succesvol verwijderd.";
            return RedirectToAction(nameof(Index));
        }
    }
}