using Microsoft.AspNetCore.Mvc;
using recipe_app_backend.Authorization;
using recipe_app_backend.Models;
using recipe_app_backend.Services;

namespace recipe_app_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]         // This controller is located at /accounts
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // Login endpoint at /login for POST requests
        [AllowAnonymous]        // Users do not need to be signed in for this endpoint
        [HttpPost("login")]
        public ActionResult<AuthenticateResponse> Login(AuthenticateRequest model)
        {
            var response = _accountService.Login(model, ipAddress());
            // Return the refresh token in a HTTP-only cookie for security
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        // The refresh token endpoint is located at /refresh-token for POST requests
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            // Get the current refresh token from the cookie to rotate it
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, ipAddress());
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        // The revoke-token endpoint is located at /revoke-token for POST requests
        [HttpPost("revoke-token")]
        public IActionResult RevokeToken()
        {
            // Accept the refresh token to be revoked from the cookie
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token)) return BadRequest(new { message = "Token is required" });

            // Users can revoke only their own tokens while admins can revoke any tokens
            if (!currentAccount.OwnsToken(token) && currentAccount.Role != Role.Admin) return Unauthorized(new { message = "Unauthorized" });

            _accountService.RevokeToken(token, ipAddress());
            return Ok(new { message = "Token revoked" });
        }

        // The register endpoint is located at /register for POST requests
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest model)
        {
            _accountService.Register(model, Request.Headers["origin"]);
            return Ok(new { message = "Account successfully created!" });
        }

        // The endpoint for getting all accounts is located at /accounts for GET requests
        [Authorize(Role.Admin)] // This is available only to admins
        [HttpGet]
        public ActionResult<List<AccountResponse>> GetAllAccounts()
        {
            var accounts = _accountService.GetAllAccounts();
            return Ok(accounts);
        }

        // The /accounts/{id} endpoint for GET requests gets the information of a specific account
        [HttpGet("{id}")]
        public ActionResult<AccountResponse> GetAccountById(Guid id)
        {
            // Users can access their own accounts while admins can access any account
            if (id != currentAccount.Id && currentAccount.Role != Role.Admin) return Unauthorized(new { message = "Unauthorized" });

            var account = _accountService.GetAccountById(id);
            return Ok(account);
        }

        // Admin-only endpoint at /accounts for POST requests to create new accounts
        [Authorize(Role.Admin)]
        [HttpPost]
        public ActionResult<AccountResponse> CreateAccount(CreateRequest model)
        {
            var account = _accountService.CreateAccount(model);
            return Ok(account);
        }

        // Updates the account with a matching id at /accounts/{id} for PUT requests
        [HttpPut("{id}")]
        public ActionResult<AccountResponse> UpdateAccount(Guid id, [FromBody] UpdateRequest model)
        {
            // Users can update their own account while admins can update any account
            if (id != currentAccount.Id && currentAccount.Role != Role.Admin) return Unauthorized(new { message = "Unauthorized" });

            // Only admins can update an account's role and ban/unban users
            if (currentAccount.Role != Role.Admin)
            {
                model.Role = null;
                model.IsBanned = null;
            }

            var account = _accountService.UpdateAccount(id, model);
            return Ok(account);
        }

        // Deletes the account with the associated id at /accounts/{id} for DELETE requests
        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(Guid id)
        {
            // Users can delete their own account while admins can delete any account
            if (id != currentAccount.Id && currentAccount.Role != Role.Admin) return Unauthorized(new { message = "Unauthorized" });

            _accountService.DeleteAccount(id);
            return Ok(new { message = "Account deleted successfully." });
        }

        // Admin-only endpoint at /users for GET requests to get all accounts with the User role
        [Authorize(Role.Admin)]
        [HttpGet("users")]
        public ActionResult<List<AccountResponse>> GetAllUsers()
        {
            return Ok(_accountService.GetAllUsers());
        }


        // Helper methods
        // Sets a refresh token in a HTTP-only cookie for security
        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        // Retrieves the client's IP address from the request headers or connection info
        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"];
            }
            else
            {
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
        }
    }
}
