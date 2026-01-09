using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class FavoriteRecipe
    {
        [Required]
        public string RecipeId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
