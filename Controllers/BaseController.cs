using Microsoft.AspNetCore.Mvc;
using recipe_app_backend.Models;

namespace recipe_app_backend.Controllers
{
    // Abstract controller that provides the currently authenticated account to all extending controllers
    [Controller]
    public abstract class BaseController : ControllerBase
    {
        // Returns the currently authenticated account in the session
        public Account currentAccount => (Account) HttpContext.Items["Account"];
    }
}
