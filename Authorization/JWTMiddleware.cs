using Microsoft.Extensions.Options;
using recipe_app_backend.Data;
using recipe_app_backend.Helpers;

namespace recipe_app_backend.Authorization
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate _next;

        public JWTMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, RecipeContext recipeContext, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();   // JWT Access token from Angular frontend
            var accountId = jwtUtils.ValidateJwtToken(token);

            if (accountId != null)
            {
                // Attach the authenticated account to the context on successful jwt validation
                // The account is retrieved from the database via the accountId from the validated JWT token
                context.Items["Account"] = await recipeContext.Accounts.FindAsync(accountId.Value);
            }
            // Otherwise no account is attached and only AllowAnonymous actions are allowed
            await _next(context);
        }

    }
}
