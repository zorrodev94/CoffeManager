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
    internal sealed record ActionLayout(Rectangle Fields,Rectangle Actions,Rectangle Primary,Rectangle Secondary);
    /// <summary>Reserves a fixed submission region and keeps both actions inside its available width.</summary>
    internal static ActionLayout WithActions(Rectangle region,int dpi,int maximumLogicalWidth)
    {
        int P(int n)=>(int)Math.Round(n*dpi/96f);
        int available=Math.Max(1,region.Width-P(48)),width=Math.Min(P(maximumLogicalWidth),available);
        bool stacked=width<P(400);int height=Math.Min(region.Height-1,P(stacked?116:72));height=Math.Max(1,height);
        var actions=new Rectangle(region.X,region.Bottom-height,region.Width,height);
        int x=Math.Max(0,(region.Width-width)/2),gap=P(12),primaryWidth=stacked?width:(width-gap)*2/3;
        var primary=new Rectangle(x,P(10),primaryWidth,P(44));
        var secondary=new Rectangle(stacked?x:x+primaryWidth+gap,stacked?P(62):P(10),stacked?width:Math.Max(1,width-primaryWidth-gap),P(44));
        return new(new Rectangle(region.X,region.Y,region.Width,Math.Max(1,region.Height-height)),actions,primary,secondary);
    }
}
