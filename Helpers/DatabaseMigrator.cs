using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Data;
using recipe_app_backend.Models;
using System;
using System.Text.Json;

namespace recipe_app_backend.Helpers
{

    public class DatabaseMigrator
    {
        // Waits for the SQL Server db and creates the RecipesDB database
        public static async Task MigrateDatabaseAsync(IServiceProvider services, ILogger logger)
        {
            const int maxRetries = 10;
            const int delaySeconds = 5;

            for (int retry = 1; retry <= maxRetries; retry++)
            {
                try
                {
                    using var scope = services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<RecipeContext>();

                    logger.LogInformation("Attempting database migration (attempt {Retry}/{MaxRetries})", retry, maxRetries);

                    await db.Database.MigrateAsync();

                    logger.LogInformation("Database migration successful");
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Database not ready yet (attempt {Retry}/{MaxRetries}). Retrying in {Delay}s...",
                        retry, maxRetries, delaySeconds);

                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new Exception("Could not connect to the database after multiple retries");
        }

        // Initialize the db with recipe data
        public static async Task SeedRecipesAsync(RecipeContext context)
        {
            // Only seed an empty db
            if (context.Recipes.Any())
            {
                return;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "RecipeData.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Seed file not found: {path}");
            }

            // Retrieve the list of recipes from the RecipeData.json file created by RecipeGetter.py
            var recipeJSONArray = await File.ReadAllTextAsync(path);
            List<Recipe> recipes = JsonSerializer.Deserialize<List<Recipe>>(recipeJSONArray)!;


            context.Recipes.AddRange(recipes);
            await context.SaveChangesAsync();
        }
    }
}
