using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        public string Voornaam { get; set; } = "";

        [Required]
        public string Achternaam { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [Display(Name = "Tijdelijk wachtwoord")]
        public string TijdelijkWachtwoord { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "";
    }
}
