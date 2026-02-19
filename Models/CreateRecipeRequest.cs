using Microsoft.Extensions.FileProviders;
using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Model class for creating recipe requests
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
        public string? ImageUrl { get; set; }       // Locates the recipe image in the backend storage
        [Required]
        public Guid? UserId { get; set; }           // The id of the user creating the recipe becomes the recipe's source
        public IFormFile? RecipeImage { get; set; }         // Image file data sent via the multipart/form-data request type
    }
}
