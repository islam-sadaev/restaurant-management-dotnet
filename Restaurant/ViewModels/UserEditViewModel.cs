using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class UserEditViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        public string Voornaam { get; set; } = "";

        [Required]
        public string Achternaam { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "";
    }
}
