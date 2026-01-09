namespace recipe_app_backend.Models
{
    public class Recipe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public string? Source { get; set; }
        public string ImageUrl { get; set; }
    }
}
