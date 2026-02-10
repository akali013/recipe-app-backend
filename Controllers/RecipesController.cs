using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Authorization;
using recipe_app_backend.Data;
using recipe_app_backend.Models;
using recipe_app_backend.Services;

namespace recipe_app_backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class RecipesController : BaseController
    {
        private IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        [HttpGet]
        public ActionResult<List<Recipe>> GetAllRecipes()
        {
            return Ok(_recipeService.GetAllRecipes());
        }

        [HttpGet("{id}")]
        public ActionResult<Recipe> GetRecipeById(string id)
        {
            return Ok(_recipeService.GetRecipeById(id));
        }

        [HttpPost]
        public ActionResult<Recipe> CreateRecipe([FromForm] CreateRecipeRequest request)    // [FromForm] allows multipart/form-data (FormData) requests
        {
            return Ok(_recipeService.CreateRecipe(request));
        }

        [HttpPut("{id}")]
        public ActionResult<Recipe> UpdateRecipe(string id, [FromForm] RecipeRequest recipe)    // [FromForm] allows multipart/form-data (FormData) requests
        {
            // Admins can edit any recipe
            if (currentAccount.Role == Role.Admin)
            {
                return Ok(_recipeService.UpdateRecipe(id, recipe));
            }

            // Users can only update their own recipes
            Guid recipeSource;

            // If the recipe is a MealDB URL, it is not from the user
            try
            {
                recipeSource = new Guid(recipe.Source!);
            }
            catch (Exception e)
            {
                return Unauthorized(new { message = "You can only update your own recipes." });
            }

            if (currentAccount.Id != recipeSource) return Unauthorized(new { message = "You can only update your own recipes." });

            return Ok(_recipeService.UpdateRecipe(id, recipe));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRecipe(string id)
        {
            _recipeService.DeleteRecipe(id);
            return Ok( new { message = "Recipe deleted successfully." });
        }

        [HttpGet("users/{id}")]
        public ActionResult<List<Recipe>> GetUserRecipes(Guid id)
        {
            List<Recipe> userRecipes = _recipeService.GetRecipesByUserId(id); 
            // Only admins or the relevant user can view a user's recipes
            if (currentAccount.Role == Role.Admin || userRecipes.All(r => r.Source == currentAccount.Id.ToString()))
            {
                return Ok(userRecipes);
            }

            return Unauthorized(new { message = "You can only view your own published recipes." });
        }
    }
}
