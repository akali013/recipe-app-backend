using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    public class UpdateRequest
    {
        private string? _password;
        private string? _confirmPassword;
        private string? _role;
        private string? _email;

        [EnumDataType(typeof(Role))]
        public string Role
        {
            get => _role ?? "";
            set => _role = replaceEmptyWithNull(value);
        }

        [EmailAddress]
        public string Email
        {
            get => _email ?? "";
            set => _email = replaceEmptyWithNull(value);
        }

        public string Password
        {
            get => _password ?? "";
            set => _password = replaceEmptyWithNull(value);
        }

        [Compare("Password")]
        public string ConfirmPassword
        {
            get => _confirmPassword ?? "";
            set => _confirmPassword = replaceEmptyWithNull(value);
        }

        public bool? IsBanned { get; set; }

        
        private string replaceEmptyWithNull(string value)
        {
            // Replace empty strings with null to make any field optional for partial updates
            return (string.IsNullOrEmpty(value) ? null : value)!;
        }
    }
}
