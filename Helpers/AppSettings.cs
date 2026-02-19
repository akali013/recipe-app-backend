namespace recipe_app_backend.Helpers
{
    // App settings that configure the app
    public class AppSettings
    {
        // For signing/verifying JWT tokens
        public string Secret { get; set; } 

        // Refresh token TTL in days, inactive tokens are deleted from the db after it
        public int RefreshTokenTTL { get; set; }
    }
}
