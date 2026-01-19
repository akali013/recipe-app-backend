using System.Text.Json.Serialization;

namespace recipe_app_backend.Models
{
    public class AuthenticateResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string? JwtToken { get; set; }

        [JsonIgnore]        // The RefreshToken is returned in a HTTP Only cookie instead of the response body
        public string? RefreshToken { get; set; }
    }
}
