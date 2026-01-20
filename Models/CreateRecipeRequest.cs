using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class CreateRecipeRequest
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Type { get; set; }
        [Required]
        public string[]? Ingredients { get; set; }
        [Required]
        public string[]? Instructions { get; set; }
        public string? ImageUrl { get; set; }
        [Required]
        public Guid? UserId { get; set; }
    }
}
