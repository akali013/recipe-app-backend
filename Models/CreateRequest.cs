using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Model class for admin requests about creating a new account
    public class CreateRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [EnumDataType(typeof(Role))]    // Confirm that this value is User or Admin
        public string Role { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]      
        [Compare("Password")]       // Ensure this matches the Password field
        public string ConfirmPassword { get; set; }
    }
}
