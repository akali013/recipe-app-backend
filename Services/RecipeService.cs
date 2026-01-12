using recipe_app_backend.Data;
using recipe_app_backend.Models;

namespace recipe_app_backend.Services
{
    public interface IRecipeService
    {
        public List<Recipe> GetAllRecipes();
        public RecipeDTO GetRecipeById(string id);
        public Recipe CreateRecipe(RecipeDTO recipe);
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

        public RecipeDTO GetRecipeById(string id)
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

            return new RecipeDTO(recipe);
        }

        public Recipe CreateRecipe(RecipeDTO dto)
        {
            AppendPeriods(dto.Instructions);
            Recipe recipe = new Recipe(dto);

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            return recipe;
        }


        // Add periods to the end of each instruction if the user did not include periods.
        private void AppendPeriods(string[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                if (!instructions[i].EndsWith("."))
                {
                    instructions[i] += ".";
                }
            }
        }
    }
}
