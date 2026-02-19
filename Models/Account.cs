using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace recipe_app_backend.Models
{
    // Database model class for user accounts
    public class Account
    {
        public Guid Id { get; set; }
        [EmailAddress]      // Ensure the email is actually an email address
        public string? Email { get; set; }
        [JsonIgnore]        // Do not return the user's password in responses
        public string? Password { get; set; }
        [EnumDataType(typeof(Role))]
        public Role Role { get; set; }          // Can be User or Admin
        public DateTime Created { get; set; }       // Creation date of the account
        public DateTime? Updated { get; set; }      // Last update date of the account
        [JsonIgnore]
        public List<RefreshToken>? RefreshTokens { get; set; }      // Refresh tokens owned by this account
        public bool IsBanned { get; set; } = false;

        // Helper method to check if the account owns the specified refresh token
        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(t => t.Token == token) != null;
        }
    }
}
