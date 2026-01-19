using Microsoft.EntityFrameworkCore;
using recipe_app_backend.Authorization;
using recipe_app_backend.Data;
using recipe_app_backend.Helpers;
using recipe_app_backend.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var AllowSpecificOrigins = "_AllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSpecificOrigins,
            policy =>
            {
                policy.WithOrigins(["http://localhost:8080", "http://localhost:4200"])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
    );
});

builder.Services.AddDbContext<RecipeContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("RecipeContext") ?? throw new InvalidOperationException("Connection string 'RecipeContext' not found."),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    // Serialize enums as strings in API responses for Roles
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure AppSettings object
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Use DI for app services
builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Migrate the database during startup so the RecipeDB database exists with the initial recipe data.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<RecipeContext>();
    await DatabaseMigrator.MigrateDatabaseAsync(app.Services, logger);
    await DatabaseMigrator.SeedRecipesAsync(context);
}

//app.UseHttpsRedirection();

app.UseCors(AllowSpecificOrigins);

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JWTMiddleware>();

app.MapControllers();

app.Run();
