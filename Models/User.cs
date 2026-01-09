using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace recipe_app_backend.Models
{
    public class User
    {
        [Required]
        public string Id { get; set; }
        public string Username { get; set; }
        [JsonIgnore]
        public string Password { get; set; }

    }
}
