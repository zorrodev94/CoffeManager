using System.Reflection;
namespace CoffeManager.Client.Admin.Presentation.Theme;
internal static class BrandResources
{
    internal static Icon AppIcon { get; }=LoadIcon();
    private static Icon LoadIcon()
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("CoffeManager.Client.Admin.Resources.Brand.coffemanager.ico")??throw new InvalidOperationException("Falta el icono de la aplicación");
        using var icon=new Icon(stream);return (Icon)icon.Clone();
    }
    internal static Image HomeCup { get; }=Load("home-cup-mint.png","CoffeManager.Client.Admin.Resources.Themes.Obsidian.Illustrations.");
    internal static Image Logo { get; }=Load("fox-coffee.png");
    internal static Image Background { get; }=Load("cafe-access.png");
    internal static Image RefinedBackground { get; }=Load("refined-access.png");
    internal static Image ObsidianBackground { get; }=Load("obsidian-access.png");
    /// <summary>
    /// Load for the current entity operation.
    /// </summary>
    /// <param name="file">The file used by this operation.</param>
    /// <returns>The result of the requested operation.</returns>
    private static Image Load(string file,string prefix="CoffeManager.Client.Admin.Resources.Brand.")
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream(prefix+file)??throw new InvalidOperationException("Falta el recurso "+file);
        using var image=Image.FromStream(stream);return new Bitmap(image);
    }
}
