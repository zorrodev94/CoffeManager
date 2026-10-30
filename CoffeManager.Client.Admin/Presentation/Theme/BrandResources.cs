using System.Reflection;
namespace CoffeManager.Client.Admin.Presentation.Theme;
internal static class BrandResources
{
    internal static Image Logo { get; }=Load("fox-coffee.png");
    internal static Image Background { get; }=Load("cafe-access.png");
    /// <summary>
    /// Load for the current entity operation.
    /// </summary>
    /// <param name="file">The file used by this operation.</param>
    /// <returns>The result of the requested operation.</returns>
    private static Image Load(string file)
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("CoffeManager.Client.Admin.Resources.Brand."+file)??throw new InvalidOperationException("Falta el recurso "+file);
        using var image=Image.FromStream(stream);return new Bitmap(image);
    }
}
