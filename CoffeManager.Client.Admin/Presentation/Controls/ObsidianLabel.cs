using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Keeps labels legible when Windows disables an owner window behind a modal dialog.</summary>
internal sealed class ObsidianLabel : Label
{
    protected override void OnPaint(PaintEventArgs e)
    {
        var flags=TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix;
        if(TextAlign is ContentAlignment.MiddleCenter or ContentAlignment.TopCenter or ContentAlignment.BottomCenter)flags|=TextFormatFlags.HorizontalCenter;
        if(TextAlign is ContentAlignment.MiddleLeft or ContentAlignment.MiddleCenter or ContentAlignment.MiddleRight)flags|=TextFormatFlags.VerticalCenter;
        if(TextAlign is ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight)flags|=TextFormatFlags.Right;
        if(TextAlign is ContentAlignment.BottomLeft or ContentAlignment.BottomCenter or ContentAlignment.BottomRight)flags|=TextFormatFlags.Bottom;
        if(AutoEllipsis)flags|=TextFormatFlags.EndEllipsis;
        TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(Padding.Left,Padding.Top,Math.Max(1,Width-Padding.Horizontal),Math.Max(1,Height-Padding.Vertical)),Enabled?ForeColor:Obsidian.Muted,flags);
    }
}
