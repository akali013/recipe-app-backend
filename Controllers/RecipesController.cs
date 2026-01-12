using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Data;
using recipe_app_backend.Models;
using recipe_app_backend.Services;

namespace recipe_app_backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecipesController : Controller
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
        public ActionResult<Recipe> CreateRecipe([FromBody] RecipeDTO dto)
        {
            return Ok(_recipeService.CreateRecipe(dto));
        }
    }
}
