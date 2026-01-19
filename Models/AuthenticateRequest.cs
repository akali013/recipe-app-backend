using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class AuthenticateRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [MinLength(8)]
        public string? Password { get; set; }
    }
}
