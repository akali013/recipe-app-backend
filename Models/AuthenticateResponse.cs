using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace recipe_app_backend.Models
{
    // Model class for the data returned in any authentication request (login, refresh token, etc.)
    public class AuthenticateResponse
    {
        public Guid Id { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [EnumDataType(typeof(Role))]
        public string? Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string? JwtToken { get; set; }       // JWT access token to be used in the Authorization header of requests

        [JsonIgnore]        // The RefreshToken is returned in a HTTP Only cookie instead of the response body
        public string? RefreshToken { get; set; }
    }
}
