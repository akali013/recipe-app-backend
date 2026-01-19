using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace recipe_app_backend.Models
{
    public class Account
    {
        public Guid Id { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [JsonIgnore]
        public string? Password { get; set; }
        public Role Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        [JsonIgnore]
        public List<RefreshToken>? RefreshTokens { get; set; }
        public bool IsBanned { get; set; } = false;

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(t => t.Token == token) != null;
        }
    }
}
