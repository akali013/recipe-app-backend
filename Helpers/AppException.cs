using System.Globalization;

namespace recipe_app_backend.Helpers
{
    // Exception class that handles any general error messages to be sent to the client.
    public class AppException : Exception
    {
        public AppException() : base() { }

        public AppException(string message) : base(message) { }

        public AppException(string message, params object[] args) : base(String.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
