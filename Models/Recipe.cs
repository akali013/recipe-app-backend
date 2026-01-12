using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class Recipe
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Ingredients { get; set; }
        [Required]
        public string Instructions { get; set; }
        public string? Source { get; set; }
        public string? ImageUrl { get; set; }

        public Recipe()
        {
            Id = Guid.NewGuid().ToString();
            Name = "";
            Type = "";
            Ingredients = "";
            Instructions = "";
            Source = "";
            ImageUrl = "";
        }

        public Recipe(RecipeDTO dto)
        {
            Id = Guid.NewGuid().ToString();
            Name = dto.Name;
            Type = dto.Type;
            Ingredients = String.Join("\\", dto.Ingredients);
            Instructions = String.Join(".", dto.Instructions);
            Source = dto.Source;
            ImageUrl = dto.ImageUrl;
        }
    }

}
