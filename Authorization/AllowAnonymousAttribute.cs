namespace recipe_app_backend.Authorization
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    {
        // This attribute allows associated controller endpoint methods
        // to be accessed without a JWT access token
    }
}
