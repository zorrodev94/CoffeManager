using System.Drawing;
namespace CoffeManager.Client.Admin.Presentation.Theme;
/// <summary>Calculates non-overlapping access regions in device pixels for a viewport and monitor DPI.</summary>
internal static class AccessLayoutMetrics
{
    internal static Size StartupSize(Size logical,Size workingArea,int dpi)
    {
        float scale=dpi/96f;
        return new Size(Math.Max(1,Math.Min((int)(logical.Width*scale),workingArea.Width-24)),Math.Max(1,Math.Min((int)(logical.Height*scale),workingArea.Height-24)));
    }
    internal static (Rectangle Fields,Rectangle Artwork) Calculate(int width,int height,int dpi)
    {
        width=Math.Max(1,width);height=Math.Max(1,height);float scale=dpi/96f;
        int artWidth=width>=900*scale?(int)(width*.42f):Math.Max(1,(int)Math.Min(width*.20f,Math.Max(48*scale,width-400*scale)));
        artWidth=Math.Min(width-1,artWidth);int left=width-artWidth;
        return(new Rectangle(0,0,left,height),new Rectangle(left,0,artWidth,height));
    }
}
