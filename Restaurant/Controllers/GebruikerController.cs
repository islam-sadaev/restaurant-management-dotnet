using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Configuration.MailService;
using Restaurant.Data.UnitOfWork; 
using Restaurant.Models;
using Restaurant.ViewModels;
using System;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace Restaurant.Controllers
{
    [Route("[controller]/[action]")]
    public class GebruikerController : Controller
    {
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;

        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public GebruikerController(
            UserManager<CustomUser> userManager,
            SignInManager<CustomUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IUnitOfWork uow,     
            IMapper mapper)      
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _uow = uow;
            _mapper = mapper;
        }

        // ============================================================
        // 1. INLOGGEN & UITLOGGEN
        // ============================================================

        [HttpGet, AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Account niet gevonden");
                return View(model);
            }

            if (!user.Actief)
            {
                ModelState.AddModelError(string.Empty, "Dit account is gedeactiveerd.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction("Index","Home");

                return RedirectToLocal(model.ReturnUrl);
            }

            ModelState.AddModelError(string.Empty, "Onjuist wachtwoord");
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ============================================================
        // 2. REGISTREREN
        // ============================================================

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> Register()
        {
            var vm = new RegisterViewModel();
            // DATA OPHALEN VIA UOW -> REPOSITORY
            vm.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
            return View(vm);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "E-mailadres bestaat al.");
                model.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                return View(model);
            }

            var confirm = _mapper.Map<RegisterConfirmViewModel>(model);

            confirm.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();

            return View("RegisterConfirm", confirm);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(RegisterConfirmViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                // Terug naar begin of confirm. Hier terug naar start:
                return View("Register", new RegisterViewModel { Landen = model.Landen });
            }

            var isFirstUser = !await _userManager.Users.AnyAsync();

            // AUTOMAPPER: ViewModel -> Entity (CustomUser)
            var user = _mapper.Map<CustomUser>(model);

            // Extra velden instellen die niet in de map zitten
            user.EmailConfirmed = true;
            user.Actief = true;
            // UserName wordt via AutoMapper profiel al ingesteld op Email

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                var regVm = new RegisterViewModel();
                regVm.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                return View("Register", regVm);
            }

            var roleName = isFirstUser ? "Eigenaar" : "Klant";
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            await _userManager.AddToRoleAsync(user, roleName);
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("TafelReserveren", "Reservatie");
        }

        // ============================================================
        // 3. ACCOUNT BEHEREN 
        // ============================================================

        [HttpGet, Authorize]
        public async Task<IActionResult> MyAccount()
        {
            var userId = _userManager.GetUserId(User);

            var user = await _uow.GebruikerRepository.GetUserByIdNoTrackingAsync(userId);

            if (user == null) return RedirectToAction("Login");

            var vm = _mapper.Map<AccountViewModel>(user);

            // Null checks
            vm.Voornaam ??= "";
            vm.Achternaam ??= "";
            vm.Adres ??= "";
            vm.Huisnummer ??= "";
            vm.Postcode ??= "";
            vm.Gemeente ??= "";

            vm.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();

            ModelState.Clear();
            return View("Account", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> MyAccount(AccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                return View("Account", model);
            }

            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return RedirectToAction("Login");

            _mapper.Map(model, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                model.Landen = await _uow.LandRepository.GetActiveLandenSelectListAsync();
                return View("Account", model);
            }

            TempData["AccountMessage"] = "Je accountgegevens zijn succesvol bijgewerkt.";
            return RedirectToAction(nameof(MyAccount));
        }

        // ============================================================
        // 4. ACCOUNT VERWIJDEREN
        // ============================================================

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (await _userManager.IsInRoleAsync(user, "Eigenaar"))
            {
                TempData["AccountError"] = "Als eigenaar kunt u uw account niet verwijderen.";
                return RedirectToAction("MyAccount");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                await _userManager.RemoveFromRolesAsync(user, roles);

            // Anonimiseren
            user.Voornaam = "<<Verwijderd>>";
            user.Achternaam = "<<Verwijderd>>";
            user.Adres = "<<Verwijderd>>";
            user.Huisnummer = "";
            user.Postcode = "";
            user.Gemeente = "";
            user.Actief = false;

            var anonEmail = $"verwijderd_{Guid.NewGuid()}@removed.local";
            user.Email = anonEmail;
            user.NormalizedEmail = anonEmail.ToUpperInvariant();
            user.UserName = anonEmail;
            user.NormalizedUserName = anonEmail.ToUpperInvariant();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["AccountError"] = "Er ging iets mis bij het verwijderen.";
                return RedirectToAction("MyAccount");
            }

            await _signInManager.SignOutAsync();
            TempData["AccountDeleted"] = "Uw account werd succesvol verwijderd.";
            return RedirectToAction("Index", "Home");
        }

        // ============================================================
        // 5. WACHTWOORD VERGETEN & RESETTEN
        // ============================================================

        [HttpGet, AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            TempData["ForgotPasswordMessage"] = "Indien het e-mailadres bestaat, ontvangt u een resetlink.";

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.Actief)
            {
                return RedirectToAction(nameof(Login));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //dit geeft Gebruiker/ResetPassword/security token, "token"/email met user.email
            var resetUrl = Url.Action(nameof(ResetPassword), "Gebruiker",
                new { token = WebUtility.UrlEncode(token), email = user.Email }, Request.Scheme);

            if (_emailSender != null)
            {
                // 1. Het onderwerp (De 'Header' van de mail in de inbox)
                string onderwerp = "Wachtwoord herstellen - The Canopy";

                // 2. De inhoud (Body) met HTML opmaak
                // De $@ zorgt ervoor dat je over meerdere regels kan typen in je code
                string bericht = $@"
                        <html>
                        <body>
                            <h3>Wachtwoord vergeten?</h3>
                            <p>Beste gebruiker,</p>
            
                            <p>Wij hebben een verzoek ontvangen om het wachtwoord van uw account (<b>{user.Email}</b>) te resetten.</p>
            
                            <p>Klik op de link hieronder om een nieuw wachtwoord in te stellen:</p>
            
                            <p>
                                <a href='{resetUrl}' style='color: blue; font-weight: bold;'>
                                   Klik hier om uw wachtwoord te resetten
                                </a>
                            </p>

                            <p><i>Link werkt niet? Kopieer en plak dan deze URL in uw browser:</i><br>
                            {resetUrl}</p>

                            <br>
                            <p>Met vriendelijke groet,<br>
                            Het Canopy Team</p>
                        </body>
                        </html>";

                // Verstuur de mail
                _emailSender.SendEmailAsync(user.Email, onderwerp, bericht);
            }
            else
            {
                // Fallback voor lokaal testen
                TempData["ResetLink"] = resetUrl;
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet, AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            var vm = new ResetPasswordViewModel
            {
                Email = email ?? string.Empty,
                Token = token ?? string.Empty
            };
            return View(vm);
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["ResetPasswordResult"] = "Wachtwoord reset aangevraagd.";
                return RedirectToAction(nameof(Login));
            }

            var decodedToken = WebUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            TempData["ResetPasswordResult"] = "Wachtwoord succesvol aangepast. U kunt nu inloggen.";
            return RedirectToAction(nameof(Login));
        }

       
        // Helpers
        

        private IActionResult RedirectToLocal(string? returnUrl)
            => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction("TafelReserveren", "Klant");
    }
}