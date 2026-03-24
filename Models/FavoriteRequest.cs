namespace recipe_app_backend.Models
{
    // Class defining the info needed for adding a favorite recipe
    public class FavoriteRequest
    {
        public string userId { get; set; }      // User id of the user adding the favorite recipe
    }
}
