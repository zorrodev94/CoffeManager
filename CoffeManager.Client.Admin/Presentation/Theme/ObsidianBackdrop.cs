using System.Drawing.Drawing2D;
namespace CoffeManager.Client.Admin.Presentation.Theme;
/// <summary>Paints restrained graphite contours without animation or per-monitor bitmap backgrounds.</summary>
internal static class ObsidianBackdrop
{
    internal static void Draw(Graphics graphics,Rectangle bounds)
    {
        if(bounds.Width<=0||bounds.Height<=0)return;
        using var fill=new LinearGradientBrush(bounds,Obsidian.Background,Color.FromArgb(21,29,31),30f);graphics.FillRectangle(fill,bounds);
        graphics.SmoothingMode=SmoothingMode.AntiAlias;using var pen=new Pen(Color.FromArgb(13,145,221,195),1);
        for(int i=0;i<7;i++)graphics.DrawBezier(pen,bounds.Left-bounds.Width*.2f,bounds.Top+i*bounds.Height*.16f,bounds.Left+bounds.Width*.3f,bounds.Top+bounds.Height*.8f+i*20,bounds.Left+bounds.Width*.7f,bounds.Top-bounds.Height*.4f+i*30,bounds.Right+bounds.Width*.1f,bounds.Top+bounds.Height*.5f+i*bounds.Height*.12f);
    }
}
