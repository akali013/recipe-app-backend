using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Models;

namespace recipe_app_backend.Data
{
    public class RecipeContext : DbContext
    {
        public RecipeContext (DbContextOptions<RecipeContext> options)
            : base(options)
        {
        }

        // Register FavoriteRecipe with a composite primary key
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FavoriteRecipe>().HasKey(recipe => new { recipe.RecipeId, recipe.AccountId });
        }

        // db tables
        public DbSet<Recipe> Recipes { get; set; } = default!;
        public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;

        // RefreshTokens is under Accounts
    }
}
