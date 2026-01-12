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
    }
}
