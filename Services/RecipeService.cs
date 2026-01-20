using Microsoft.AspNetCore.Mvc;
using recipe_app_backend.Data;
using recipe_app_backend.Models;

namespace recipe_app_backend.Services
{
    public interface IRecipeService
    {
        public List<Recipe> GetAllRecipes();
        public RecipeDTO GetRecipeById(string id);
        public Recipe CreateRecipe(CreateRecipeRequest recipe);
        public void DeleteRecipe(string id);
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


            return new RecipeDTO(recipe);
        }

        public Recipe CreateRecipe(CreateRecipeRequest request)
        {
            AppendPeriods(request.Instructions!);
            Recipe recipe = new Recipe(request);

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            return recipe;
        }

        public void DeleteRecipe(string id)
        {
            Recipe? deletedRecipe = _context.Recipes.SingleOrDefault(r => r.Id == id);
            
            if (deletedRecipe == null)
            {
                throw new KeyNotFoundException("Recipe not found!");
            }

            _context.Recipes.Remove(deletedRecipe);
            _context.SaveChanges();
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
