using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws a local eye glyph without emoji fonts or external assets.</summary>
internal sealed class PasswordVisibilityButton : Button
{
    private bool _revealed;
    internal void SetRevealed(bool revealed){_revealed=revealed;Invalidate();}
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
        float s=DeviceDpi/96f,cx=Width/2f,cy=Height/2f;
        using var pen=new Pen(Enabled?Obsidian.Muted:Obsidian.Border,1.5f*s);
        using var eye=new GraphicsPath();eye.AddBezier(cx-10*s,cy,cx-5*s,cy-9*s,cx+5*s,cy-9*s,cx+10*s,cy);eye.AddBezier(cx+10*s,cy,cx+5*s,cy+9*s,cx-5*s,cy+9*s,cx-10*s,cy);
        e.Graphics.DrawPath(pen,eye);e.Graphics.DrawEllipse(pen,cx-3*s,cy-3*s,6*s,6*s);
        if(_revealed)e.Graphics.DrawLine(pen,cx-10*s,cy+9*s,cx+10*s,cy-9*s);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-4,-4));
    }
}
