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

        public AuthenticateResponse Login(AuthenticateRequest model, string ipAddress)
        {
            Account account = _context.Accounts.SingleOrDefault(a => a.Email == model.Email);

            if (account == null|| !Encryption.Verify(model.Password, account.Password))
            {
                throw new AppException("Email or password is incorrect");
            }

            if (account.IsBanned)
            {
                throw new AppException("You are banned.");
            }

            // Generate JWT and refresh tokens upon successful login
            var jwtToken = _jwtUtils.GenerateJwtToken(account);
            var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
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

            // Generate a new access token
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

        public void RevokeToken(string token, string ipAddress)
        {
            Account account = getAccountByRefreshToken(token);
            var refreshToken = account.RefreshTokens.Single(t => t.Token == token);

            if (!refreshToken.IsActive) throw new AppException("Invalid token");

            revokeRefreshToken(refreshToken, ipAddress, "Revoked by user request");
            _context.Update(account);
            _context.SaveChanges();
        }

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

        public AccountResponse CreateAccount(CreateRequest model)
        {
            if (_context.Accounts.Any(a => a.Email == model.Email)) throw new AppException($"Email '{model.Email}' is already taken.");

            Account account = new Account
            {
                Id = Guid.NewGuid(),
                Email = model.Email,
                Password = Encryption.HashPassword(model.Password),
                Created = DateTime.UtcNow,
                IsBanned = false
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

        public AccountResponse UpdateAccount(Guid id, UpdateRequest model)
        {
            Account account = getAccount(id);

            if (account.Email != model.Email && _context.Accounts.Any(a => a.Email == model.Email))
                throw new AppException($"Email '{model.Email}' is already taken");

            if (!string.IsNullOrEmpty(model.Password))
                account.Password = Encryption.HashPassword(model.Password);

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

        public void DeleteAccount(Guid id)
        {
            Account account = getAccount(id);
            _context.Accounts.Remove(account);
            _context.SaveChanges();
        }


        private Account getAccount(Guid id)
        {
            Account? account = _context.Accounts.SingleOrDefault(a => a.Id == id);
            if (account == null) throw new KeyNotFoundException("Account not found");
            return account;
        }

        private Account getAccountByRefreshToken(string token)
        {
            var account = _context.Accounts.SingleOrDefault(a => a.RefreshTokens.Any(t => t.Token == token));
            if (account == null) throw new AppException("Invalid refresh token");
            return account;
        }

        private RefreshToken rotateRefreshToken(RefreshToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
            revokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private void removeOldRefreshTokens(Account account)
        {
            account.RefreshTokens.RemoveAll(t =>
                !t.IsActive &&
                t.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }

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

        private void revokeRefreshToken(RefreshToken token, string ipAddress, string reason = null, string replacedByToken = null)
        {
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }
    }
}
