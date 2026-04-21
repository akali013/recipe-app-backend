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
        public List<Recipe> GetFavoriteRecipes(Guid userId);
        public Recipe ToggleFavoriteRecipe(string recipeId, Guid userId);
    }

    public class RecipeService : IRecipeService
    {
        private readonly RecipeContext _context;
        private readonly IWebHostEnvironment _env;      // Used to retrieve image file directory
        private string[] allowedTypes = [               // Allowed image file types for recipe image files
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

        // Get all recipes in the database
        public List<Recipe> GetAllRecipes()
        {
            return _context.Recipes.ToList();
        }

        // Get the information for the recipe under the given id
        public Recipe GetRecipeById(string id)
        {
            Recipe? recipe = _context.Recipes.SingleOrDefault(recipe => recipe.Id == id);

            if (recipe == null)
            {
                throw new KeyNotFoundException("Recipe not found!");
            }

            return recipe;
        }

        // Creates a user-submitted recipe in the database 
        public Recipe CreateRecipe(CreateRecipeRequest request)
        {
            AppendPeriods(request.Instructions!);
            Recipe recipe = new Recipe(request);        // Create a new recipe object from the request

            // Create the recipe image in the backend storage and save the file location in the database
            if (request.RecipeImage != null)
            {
                // Verify that the new image file has a valid image type
                if (!allowedTypes.Contains(request.RecipeImage.ContentType))
                {
                    throw new AppException("Recipe image file must have a valid image type. Examples: .png, .jpg, .gif, etc.");
                }

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

        // Updates the recipe under the given id in the database
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

            _context.Recipes.Update(recipeToUpdate);
            _context.SaveChanges();

            return recipeToUpdate;
        }

        // Deletes the recipe under the given id from the database
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

        // Gets the recipes created by the user under the given id from the database
        public List<Recipe> GetRecipesByUserId(Guid id)
        {
            List<Recipe> recipes = _context.Recipes.Where(recipe => recipe.Source == id.ToString()).ToList();
            return recipes;
        }

        // Retrieves all recipes favorited by the user with the matching userId
        public List<Recipe> GetFavoriteRecipes(Guid userId)
        {
            List<FavoriteRecipe> favoritedRecipes = _context.FavoriteRecipes.Where(r => r.AccountId == userId).ToList();
            List<Recipe> recipes = favoritedRecipes.Select(fav => _context.Recipes.SingleOrDefault(r => r.Id == fav.RecipeId)).ToList()!;

            return recipes;
        }

        // Adds/Removes the recipe under the given recipeId to/from the specified user's list of favorite recipes
        public Recipe ToggleFavoriteRecipe(string recipeId, Guid userId)
        {
            Recipe? recipe = _context.Recipes.SingleOrDefault(r => r.Id == recipeId);

            if (recipe == null)
            {
                throw new AppException("Recipe not found!");
            }

            // Check if the recipe is favorited or not by the user
            FavoriteRecipe? favoritedRecipe = _context.FavoriteRecipes.SingleOrDefault(r => r.RecipeId == recipe.Id && r.AccountId == userId);
            if (favoritedRecipe == null)
            {
                // Add the new favorite recipe
                FavoriteRecipe newFavorite = new FavoriteRecipe
                {
                    RecipeId = recipe.Id,
                    AccountId = userId
                };

                _context.FavoriteRecipes.Add(newFavorite);
            }
            else
            {
                // Remove the favorite recipe from the user's list
                _context.FavoriteRecipes.Remove(favoritedRecipe);
            }

            _context.SaveChanges();

            return recipe;
        }



        // Helper methods
        // Add periods to the end of each instruction if the user did not include periods in their submitted recipe
        private void AppendPeriods(string[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                if (!instructions[i].Trim().EndsWith("."))
                {
                    instructions[i] += ".";
                }
            }
        }

        // Creates a new recipe image in the wwwroot/recipeImages directory and returns the file name
        // Essentially, store the actual file in the backend and the file name in the database
        private string createRecipeImage(RecipeRequest recipe)
        {
            // Delete the old image if it exists
            if (recipe.ImageUrl != null && File.Exists(Path.Combine(_env.WebRootPath, "recipeImages", recipe.ImageUrl)))
            {
                File.Delete(Path.Combine(_env.WebRootPath, "recipeImages", recipe.ImageUrl));
            }

            // Generate a unique filename for the image and save it at the wwwroot/recipeImages directory
            // Filename = Guid + file extension
            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(recipe.RecipeImage!.FileName)}";
            var dirPath = Path.Combine(_env.WebRootPath, "recipeImages");

            // Create the wwwroot/recipeImages directory if it does not exist
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
