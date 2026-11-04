using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws the local CoffeManager wordmark independently of the decorative access photograph.</summary>
internal sealed class AccessBrand : Control
{
    internal AccessBrand(){AccessibleName="CoffeManager";Margin=new Padding(0,0,0,16);TabStop=false;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f;
        using var pen=new Pen(Obsidian.Mint,1.8f*s){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
        e.Graphics.DrawArc(pen,3*s,13*s,25*s,19*s,0,180);e.Graphics.DrawLine(pen,3*s,13*s,28*s,13*s);e.Graphics.DrawArc(pen,23*s,13*s,12*s,12*s,270,180);e.Graphics.DrawBezier(pen,12*s,9*s,5*s,2*s,23*s,5*s,17*s,0);
        using var font=new Font("Segoe UI Semibold",12);TextRenderer.DrawText(e.Graphics,"CoffeManager",font,new Rectangle((int)(48*s),0,Math.Max(1,Width-(int)(48*s)),Height),Obsidian.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
    }
}
