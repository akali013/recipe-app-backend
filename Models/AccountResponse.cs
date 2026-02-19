using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Account data to be sent in any response to the client
    public class AccountResponse
    {
        public Guid Id { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [EnumDataType(typeof(Role))]
        public string? Role { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool IsBanned { get; set; }
    }
}
