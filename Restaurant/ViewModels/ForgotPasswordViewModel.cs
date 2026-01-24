using System.ComponentModel.DataAnnotations;

namespace Restaurant.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
