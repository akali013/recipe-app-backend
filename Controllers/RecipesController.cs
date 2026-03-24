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
    [Route("[controller]")]     // This controller endpoint is at /recipes
    public class RecipesController : BaseController
    {
        private IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET requests to /recipes returns all recipes in the database
        [HttpGet]
        public ActionResult<List<Recipe>> GetAllRecipes()
        {
            return Ok(_recipeService.GetAllRecipes());
        }

        // GET requests to /recipes/{id} returns the recipe with the specified id
        [HttpGet("{id}")]
        public ActionResult<Recipe> GetRecipeById(string id)
        {
            return Ok(_recipeService.GetRecipeById(id));
        }

        // POST requests to /recipes creates a new recipe
        [HttpPost]
        public ActionResult<Recipe> CreateRecipe([FromForm] CreateRecipeRequest request)    // [FromForm] allows multipart/form-data (FormData) requests
        {
            return Ok(_recipeService.CreateRecipe(request));
        }

        // PUT requests to /recipes/{id} updates the recipe with the associated id
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

            // If the recipe source is a MealsDB API URL, it is not from the user
            // otherwise, it must be the id of the user that created it
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

        // DELETE requests to /recipes/{id} deletes the recipe with the specified id
        [HttpDelete("{id}")]
        public IActionResult DeleteRecipe(string id)
        {
            _recipeService.DeleteRecipe(id);
            return Ok( new { message = "Recipe deleted successfully." });
        }

        // GET requests to /users/{id} returns all recipes created by the user with the specified id
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

        // GET requests to /favorite/{id} returns all recipes favorited by the specified user
        [HttpGet("favorite/{id}")]
        public ActionResult<List<Recipe>> GetFavoriteRecipes(string id)
        {
            Guid userId = new Guid(id);
            return Ok(_recipeService.GetFavoriteRecipes(userId));
        }

        // POST requests to /favorite/{id} adds/removes the recipe with the specified id to/from the specified user's favorite recipes
        [HttpPost("favorite/{id}")]
        public ActionResult<Recipe> ToggleFavoriteRecipe(string id, [FromBody] FavoriteRequest request)
        {
            Guid userGuid = new Guid(request.userId);
            return Ok(_recipeService.ToggleFavoriteRecipe(id, userGuid));
        }
    }
}
