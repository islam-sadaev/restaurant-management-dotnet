using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class LoginViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Wachtwoord")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Onthoud mij")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
