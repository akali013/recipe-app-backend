using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Database model class for the Recipes table
    public class Recipe
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string[]? Ingredients { get; set; }
        [Required]
        public string[]? Instructions { get; set; }
        public string? Source { get; set; }         // Can be a MealDB API URL or a user id
        public string? ImageUrl { get; set; }

        public Recipe()
        {
            Id = Guid.NewGuid().ToString();
            Name = "";
            Type = "";
            Ingredients = [];
            Instructions = [];
            Source = "";
            ImageUrl = "";
        }

        // Convert a user's recipe creation request into a recipe
        public Recipe(CreateRecipeRequest request)
        {
            Id = Guid.NewGuid().ToString();
            Name = request.Name!;
            Type = request.Type!;
            Ingredients = request.Ingredients;
            Instructions = request.Instructions!;
            Source = request.UserId.ToString();
            ImageUrl = request.ImageUrl;
        }
    }

}
