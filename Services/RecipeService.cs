using Microsoft.AspNetCore.Mvc;
using recipe_app_backend.Data;
using recipe_app_backend.Helpers;
using recipe_app_backend.Models;
using System.Threading.Tasks;

namespace recipe_app_backend.Services
{
    public interface IRecipeService
    {
        public List<Recipe> GetAllRecipes();
        public Recipe GetRecipeById(string id);
        public Recipe CreateRecipe(CreateRecipeRequest recipe);
        public Recipe UpdateRecipe(string id, RecipeRequest recipe);
        public void DeleteRecipe(string id);
        public List<Recipe> GetRecipesByUserId(Guid id);
    }

    public class RecipeService : IRecipeService
    {
        private readonly RecipeContext _context;
        private readonly IWebHostEnvironment _env;      // Used to retrieve image file directory
        private string[] allowedTypes = [
            "image/apng",
            "image/bmp",
            "image/gif",
            "image/jpeg",
            "image/pjpeg",
            "image/png",
            "image/svg+xml",
            "image/tiff",
            "image/webp",
            "image/x-icon"
        ];

        public RecipeService(RecipeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public List<Recipe> GetAllRecipes()
        {
            return _context.Recipes.ToList();
        }

        public Recipe GetRecipeById(string id)
        {
            Recipe? recipe = _context.Recipes.SingleOrDefault(recipe => recipe.Id == id);

            if (recipe == null)
            {
                throw new KeyNotFoundException("Recipe not found!");
            }

            // Create lists of ingredients and instructions
            recipe.Ingredients = recipe.Ingredients![0].Split(",");
            recipe.Instructions = recipe.Instructions![0].Split(",");


            return recipe;
        }

        public Recipe CreateRecipe(CreateRecipeRequest request)
        {
            AppendPeriods(request.Instructions!);
            Recipe recipe = new Recipe(request);

            // Create the recipe image in the backend storage and save the file name in the db
            if (request.RecipeImage != null)
            {
                recipe.ImageUrl = createRecipeImage(new RecipeRequest
                {
                    Name = request.Name,
                    Type = request.Type,
                    Ingredients = request.Ingredients,
                    Instructions = request.Instructions,
                    Source = request.UserId.ToString(),
                    ImageUrl = request.ImageUrl,
                    RecipeImage = request.RecipeImage
                });
            }

            _context.Recipes.Add(recipe);
            _context.SaveChanges();

            return recipe;
        }

        public Recipe UpdateRecipe(string id, RecipeRequest recipe)
        {
            Recipe? recipeToUpdate = _context.Recipes.SingleOrDefault(r => r.Id == id);

            if (recipeToUpdate == null)
            {
                throw new KeyNotFoundException("Recipe not found!");
            }

            // Validate that any submitted file is an image
            if (recipe.RecipeImage != null)
            {
                if (!allowedTypes.Contains(recipe.RecipeImage.ContentType))
                {
                    throw new AppException("Recipe image file must have a valid image type. Examples: .png, .jpg, .gif, etc.");
                }

                // Save the recipe's image url to the database
                recipeToUpdate.ImageUrl = createRecipeImage(recipe);
            }
            
            // Update other recipe info
            recipeToUpdate.Name = recipe.Name;
            recipeToUpdate.Type = recipe.Type;
            recipeToUpdate.Ingredients = recipe.Ingredients;
            recipeToUpdate.Instructions = recipe.Instructions;
            recipeToUpdate.Source = recipe.Source;

            _context.Update(recipeToUpdate);
            _context.SaveChanges();

            return recipeToUpdate;
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

        public List<Recipe> GetRecipesByUserId(Guid id)
        {
            List<Recipe> recipes = _context.Recipes.Where(recipe => recipe.Source == id.ToString()).ToList();
            return recipes;
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

        // Creates a new recipe image in the wwwroot/recipeImages directory and returns the file name
        // Essentially, store the actual file in the backend and the file name in the db
        private string createRecipeImage(RecipeRequest recipe)
        {
            // Delete the old image if it exists
            if (recipe.ImageUrl != null && File.Exists(Path.Combine(_env.WebRootPath, "recipeImages", recipe.ImageUrl)))
            {
                File.Delete(Path.Combine(_env.WebRootPath, "recipeImages", recipe.ImageUrl));
            }

            // Generate a unique filename for the image and save it at the wwwroot/recipeImages directory
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(recipe.RecipeImage!.FileName)}";
            var dirPath = Path.Combine(_env.WebRootPath, "recipeImages");

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            var filePath = Path.Combine(dirPath, fileName);

            // Upload the file to the wwwroot/recipeImages directory under the unique file name
            using var stream = new FileStream(filePath, FileMode.Create);
            recipe.RecipeImage!.CopyTo(stream);
            
            return fileName;
        }
    }
}
