using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using recipe_app_backend.Models;

namespace recipe_app_backend.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<Role> _requiredRoles;
        
        public AuthorizeAttribute(params Role[] roles)
        {
            _requiredRoles = roles ?? new Role[] { };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if the associated controller action has the [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous) return;

            // Check if the user is logged in
            var account = (Account) context.HttpContext.Items["Account"];
            if (account == null || (_requiredRoles.Any() && !_requiredRoles.Contains(account.Role)))
            {
                // If the user is not logged in or doesn't have access, send a 401 Unauthorized response
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
