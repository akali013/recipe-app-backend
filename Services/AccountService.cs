using Microsoft.Extensions.Options;
using recipe_app_backend.Authorization;
using recipe_app_backend.Data;
using recipe_app_backend.Helpers;
using recipe_app_backend.Models;
using Encryption = BCrypt.Net.BCrypt;

namespace recipe_app_backend.Services
{
    public interface IAccountService
    {
        AuthenticateResponse Login(AuthenticateRequest model, string ipAddress);
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        void RevokeToken(string token, string ipAddress);
        AccountResponse Register(RegisterRequest model, string origin);
        List<AccountResponse> GetAllAccounts();
        AccountResponse GetAccountById(Guid id);
        AccountResponse CreateAccount(CreateRequest model);
        AccountResponse UpdateAccount(Guid id, UpdateRequest model);
        void DeleteAccount(Guid id);
        List<AccountResponse> GetAllUsers();
    }


    public class AccountService : IAccountService
    {
        private readonly RecipeContext _context;
        private readonly IJwtUtils _jwtUtils;
        private readonly AppSettings _appSettings;

        public AccountService(RecipeContext context, IJwtUtils jwtUtils, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _appSettings = appSettings.Value;
        }

        // Logs the user in the app with a matching email and password
        public AuthenticateResponse Login(AuthenticateRequest model, string ipAddress)
        {
            Account account = _context.Accounts.SingleOrDefault(a => a.Email == model.Email);

            // Check if the email is registered and that the password is correct
            if (account == null|| !Encryption.Verify(model.Password, account.Password))
            {
                throw new AppException("Email or password is incorrect");
            }

            if (account.IsBanned)
            {
                throw new AppException("You are banned.");
            }

            // Generate JWT access and refresh tokens upon successful login
            var jwtToken = _jwtUtils.GenerateJwtToken(account);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            // Add the new refresh token to the account in the database and remove old tokens
            account.RefreshTokens.Add(refreshToken);
            removeOldRefreshTokens(account);

            _context.Update(account);
            _context.SaveChanges();

            return new AuthenticateResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            };
        }

        // Generate new JWT access and refresh tokens while rotating the old token
        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            Account account = getAccountByRefreshToken(token);
            var refreshToken = account.RefreshTokens.Single(t => t.Token == token);

            if (refreshToken.IsRevoked)
            {
                // Revoke all descendant tokens in case this token has been compromised
                revokeDescendantRefreshTokens(refreshToken, account, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
                _context.Update(account);
                _context.SaveChanges();
            }

            if (!refreshToken.IsActive) throw new AppException("Invalid token");

            // Rotate old refresh tokens
            var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
            account.RefreshTokens.Add(newRefreshToken);

            // Remove old refresh tokens from account
            removeOldRefreshTokens(account);

            _context.Update(account);
            _context.SaveChanges();

            // Generate a new JWT access token
            var jwtToken = _jwtUtils.GenerateJwtToken(account);

            return new AuthenticateResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        // Logs the user out of the app by revoking their refresh token
        public void RevokeToken(string token, string ipAddress)
        {
            Account account = getAccountByRefreshToken(token);
            var refreshToken = account.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive) throw new AppException("Invalid token");

            revokeRefreshToken(refreshToken, ipAddress, "Revoked by user request");
            _context.Update(account);
            _context.SaveChanges();
        }

        // Creates a new account with the provided email and password
        public AccountResponse Register(RegisterRequest model, string origin)
        {
            if (_context.Accounts.Any(a => a.Email == model.Email))
            { 
                throw new AppException("This email is already registered.");
            }

            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                Password = Encryption.HashPassword(model.Password),  
            };

            // The first registered account is an admin
            var isFirstAccount = !_context.Accounts.Any();
            account.Role = isFirstAccount ? Role.Admin : Role.User;
            account.Created = DateTime.UtcNow;

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = false
            };
        }

        // Return all accounts in the database without their passwords
        public List<AccountResponse> GetAllAccounts()
        {
            return _context.Accounts.ToList().Select(account => new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = account.IsBanned
            }).ToList();
        }

        // Returns the information of the account with the specified id without its password
        public AccountResponse GetAccountById(Guid id)
        {
            Account account = getAccount(id);
            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = account.IsBanned
            };
        }

        // Admin-only method for creating user accounts in the database
        public AccountResponse CreateAccount(CreateRequest model)
        {
            // Check if the requested email is already used
            if (_context.Accounts.Any(a => a.Email == model.Email)) throw new AppException($"Email '{model.Email}' is already taken.");

            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                Password = Encryption.HashPassword(model.Password),
                Created = DateTime.UtcNow,
                IsBanned = false,
                Role = Role.User        // Admins are not allowed to create other admin accounts
            };

            _context.Accounts.Add(account);
            _context.SaveChanges();

            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = false
            };
        }

        // Updates the account under id with any data in the UpdateRequest model parameter
        public AccountResponse UpdateAccount(Guid id, UpdateRequest model)
        {
            Account account = getAccount(id);

            // Check that any new emails are not used by another account
            if (account.Email != model.Email && _context.Accounts.Any(a => a.Email == model.Email))
                throw new AppException($"Email '{model.Email}' is already taken");

            // Replace any values with new ones from the model
            if (!string.IsNullOrEmpty(model.Password))
                account.Password = Encryption.HashPassword(model.Password);

            if (!string.IsNullOrEmpty(model.Email))
                account.Email = model.Email;

            account.IsBanned = model.IsBanned ?? account.IsBanned;
            account.Updated = DateTime.UtcNow;

            _context.Accounts.Update(account);
            _context.SaveChanges();

            return new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = account.IsBanned
            };
        }

        // Deletes an account under the id parameter from the database
        public void DeleteAccount(Guid id)
        {
            Account account = getAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }

        // Retrieves a list of all user accounts in the database without their passwords
        public List<AccountResponse> GetAllUsers()
        {
            List<Account> users = _context.Accounts.Where(a => a.Role == Role.User).ToList();

            return users.Select(account => new AccountResponse
            {
                Id = account.Id,
                Email = account.Email,
                Role = account.Role.ToString(),
                Created = account.Created,
                Updated = account.Updated,
                IsBanned = account.IsBanned
            }).ToList();
        }

        // Helper methods
        // Returns an Account object with a matching id from the database
        private Account getAccount(Guid id)
        {
            Account? account = _context.Accounts.SingleOrDefault(a => a.Id == id);
            if (account == null) throw new KeyNotFoundException("Account not found");
            return account;
        }

        // Returns the Account object that contains the provided refresh token string in its RefreshTokens list
        private Account getAccountByRefreshToken(string token)
        {
            var account = _context.Accounts.SingleOrDefault(a => a.RefreshTokens.Any(t => t.Token == token));
            if (account == null) throw new AppException("Invalid refresh token");       // No matching refresh token was found, so the given token must be invalid
            return account;
        }

        // Rotate the given refresh token by revoking it and returning a new refresh token to replace it
        private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        // Remove old and inactive refresh tokens from the given account's RefreshTokens list in the database
        private void removeOldRefreshTokens(Account account)
        {
            account.RefreshTokens.RemoveAll(t =>
                !t.IsActive &&
                t.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }

        // If a revoked token is attempted to be reused, revoke all its descendant tokens for security
        private void revokeDescendantRefreshTokens(RefreshToken refreshToken, Account account, string ipAddress, string reason)
        {
            // Recursively traverse the refresh token chain and ensure all descendants are revoked
            if (!string.IsNullOrEmpty(refreshToken.ReplacedByToken))
            {
                var childToken = account.RefreshTokens.SingleOrDefault(a => a.Token == refreshToken.ReplacedByToken);

                if (childToken.IsActive)
                    revokeRefreshToken(childToken, ipAddress, reason);
                else
                    revokeDescendantRefreshTokens(childToken, account, ipAddress, reason);
            }
        }

        // Revoke a refresh token by setting its revoked properties
        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
    }
}
