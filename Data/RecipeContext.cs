using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Models;

namespace recipe_app_backend.Data
{
    // This class represents the database
    public class RecipeContext : DbContext
    {
        public RecipeContext (DbContextOptions<RecipeContext> options)
            : base(options)
        {
        }

        // Register the FavoriteRecipes table with a composite primary key consisting of the recipeId and the accountId
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FavoriteRecipe>().HasKey(recipe => new { recipe.RecipeId, recipe.AccountId });
        }

        // Database tables
        public DbSet<Recipe> Recipes { get; set; } = default!;      // Recipes table
        public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; } = default!;      // FavoriteRecipes table
        public DbSet<Account> Accounts { get; set; } = default!;        // Accounts table

        // RefreshTokens is under Accounts due to its [Owned] attribute
    }
}
