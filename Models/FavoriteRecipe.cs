using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Database model class for the FavoriteRecipes table
    public class FavoriteRecipe
    {
        [Required]
        public string? RecipeId { get; set; }       // Recipe being favorited
        [Required]
        public Guid? AccountId { get; set; }      // User who favorited the recipe
    }
}
