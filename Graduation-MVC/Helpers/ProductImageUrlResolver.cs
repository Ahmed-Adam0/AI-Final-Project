namespace Graduation_MVC.Helpers
{
    /// <summary>
    /// Resolves product image paths from the database to MVC-relative URLs served by wwwroot.
    /// </summary>
    public static class ProductImageUrlResolver
    {
        public const string NoImagePath = "/images/no-image.png";

        public static string Resolve(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return NoImagePath;

            path = path.Trim();

            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return path;

            if (path.StartsWith('/'))
                return "http://home-ai.runasp.net/" + path;

            //if (path.Contains('/'))
            //    return "/" + path.TrimStart('/');

            //return "/uploads/products/" + path;
            return "http://home-ai.runasp.net/" + path;
        }
    }
}
