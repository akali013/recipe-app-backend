using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class RecipeResponse
    {
        [Required]
        public string Id { get; set; }
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

        // Convert Recipe to RecipeResponse by separating instructions by periods and ingredients by backslashes
        public RecipeResponse(Recipe recipe)
        {
            Id = recipe.Id;
            Name = recipe.Name;
            Type = recipe.Type;
            Ingredients = recipe.Ingredients.Split("\\", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Instructions = recipe.Instructions.Split(".", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Source = recipe.Source;
            ImageUrl = recipe.ImageUrl;
        }
    }
}
