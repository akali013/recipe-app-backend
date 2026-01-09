using recipe_app_backend.Data;
using recipe_app_backend.Models;

namespace recipe_app_backend.Services
{
    public interface IRecipeService
    {
        public List<Recipe> GetAllRecipes();
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
    }
}
