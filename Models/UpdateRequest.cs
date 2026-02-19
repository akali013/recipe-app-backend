using System.ComponentModel.DataAnnotations;

namespace recipe_app_backend.Models
{
    // Model class for requests updating accounts
    public class UpdateRequest
    {
        // Instance variables for the account attributes that can be optionally updated
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

        [MinLength(8)]
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


        // Replace empty strings with null to make any field optional for partial updates
        private string replaceEmptyWithNull(string value)
        {
            return (string.IsNullOrEmpty(value) ? null : value)!;
        }
    }
}
