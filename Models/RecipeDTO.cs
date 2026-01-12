using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class RecipeDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string[] Ingredients { get; set; }
        [Required]
        public string[] Instructions { get; set; }
        public string? Source { get; set; }
        public string? ImageUrl { get; set; }

        public RecipeDTO()
        {
            Name = "";
            Type = "";
            Ingredients = [];
            Instructions = [];
            Source = "";
            ImageUrl = "";
        }

        // Convert Recipe to RecipeDTO by separating instructions by periods and ingredients by backslashes
        public RecipeDTO(Recipe recipe)
        {
            Name = recipe.Name;
            Type = recipe.Type;
            Ingredients = recipe.Ingredients.Split("\\", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Instructions = recipe.Instructions.Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Source = recipe.Source;
            ImageUrl = recipe.ImageUrl;
        }
    }
}
