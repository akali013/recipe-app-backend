using recipe_app_backend.Data;
using recipe_app_backend.Models;

namespace recipe_app_backend.Services
{
    public interface IRecipeService
    {
        public List<Recipe> GetAllRecipes();
        public RecipeResponse GetRecipeById(string id);
    }

    public class RecipeService : IRecipeService
    {
        private readonly RecipeContext _context;
        public RecipeService(RecipeContext context)
        {
            _context = context;
        }
        
        public List<Recipe> GetAllRecipes()
        {
            return _context.Recipes.ToList();
        }

        public RecipeResponse GetRecipeById(string id)
        {
            Recipe? recipe = _context.Recipes.SingleOrDefault(recipe => recipe.Id == id);
            
            if (recipe == null)
            {
                throw new KeyNotFoundException("Recipe not found!");
            }

            // Return the API's data source URL if no source is provided
            if (recipe.Source == null)
            {
                recipe.Source = "https://www.themealdb.com/meal/" + recipe.Id;
            }

            return new RecipeResponse(recipe);
        }
    }
}
