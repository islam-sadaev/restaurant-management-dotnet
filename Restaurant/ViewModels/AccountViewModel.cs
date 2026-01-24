using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class AccountViewModel
    {
        [Required]
        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Achternaam")]
        public string Achternaam { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-mailadres")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string? Adres { get; set; }

        [Display(Name = "Huisnummer")]
        public string? Huisnummer { get; set; }

        [Display(Name = "Postcode")]
        public string? Postcode { get; set; }

        [Display(Name = "Gemeente")]
        public string? Gemeente { get; set; }

        [Required]
        [Display(Name = "Land")]
        public int LandId { get; set; }

        public List<SelectListItem> Landen { get; set; } = new();
    }
}
