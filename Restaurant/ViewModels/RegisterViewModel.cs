using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Restaurant.ViewModels
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = string.Empty;

        [Required, Display(Name = "Achternaam")]
        public string Achternaam { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Bevestig wachtwoord")]
        [Compare(nameof(Password), ErrorMessage = "Wachtwoorden komen niet overeen.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required] public string Adres { get; set; } = string.Empty;
        [Required] public string Huisnummer { get; set; } = string.Empty;
        [Required] public string Postcode { get; set; } = string.Empty;
        [Required] public string Gemeente { get; set; } = string.Empty;

        [Required, Display(Name = "Land")]
        public int LandId { get; set; }

        // voor de dropdown lijst
        public List<SelectListItem> Landen { get; set; } = new();
        public bool Nieuwsbrief { get; set; }
    }
}
