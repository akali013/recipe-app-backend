using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    [Owned]     // This database model class is owned by the Account class, so it will have its own table, but can only be referenced through associated accounts
    public class RefreshToken
    {
        [Key]       // Specify the id as the primary key for the RefreshTokens table
        public int Id { get; set; }
        public Account? Account { get; set; }
        public string? Token { get; set; }      // Text representation of the refresh token
        public DateTime Expires { get; set; }   // Expiration date of the refresh token
        public DateTime Created { get; set; }   // Creation date of the refresh token
        public string? CreatedByIp { get; set; }    // IP address from where the token was created
        public DateTime? Revoked { get; set; }      // Revocation date of the refresh token
        public string? RevokedByIp { get; set; }    // IP address from where the token was revoked
        public string? ReplacedByToken { get; set; }    // Successor token that replaces this token for auditing purposes
        public string? ReasonRevoked { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsRevoked => Revoked != null;
        public bool IsActive => Revoked == null && !IsExpired;      // Active tokens are not revoked or expired
    }
}
