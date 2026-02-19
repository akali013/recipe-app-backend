using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Model class for the data sent in any login request
    public class AuthenticateRequest
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [MinLength(8)]      // Passwords must be at least 8 characters long
        public string? Password { get; set; }
    }
}
