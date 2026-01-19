namespace recipe_app_backend.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        // Refresh token TTL in days, inactive tokens are deleted from the db after it
        public int RefreshTokenTTL { get; set; }
    }
}
