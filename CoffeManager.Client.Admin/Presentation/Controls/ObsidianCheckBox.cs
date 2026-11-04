using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Preserves native checkbox interaction while drawing readable dark states.</summary>
internal sealed class ObsidianCheckBox : CheckBox
{
    internal ObsidianCheckBox(){FlatStyle=FlatStyle.Flat;UseVisualStyleBackColor=false;AutoSize=false;ForeColor=Obsidian.Text;BackColor=Obsidian.Background;Font=new Font("Segoe UI",10);SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
    public override Size GetPreferredSize(Size proposedSize)
    {
        int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);int width=proposedSize.Width>0?proposedSize.Width:P(460);
        var text=TextRenderer.MeasureText(Text,Font,new Size(Math.Max(1,width-P(34)),int.MaxValue),TextFormatFlags.WordBreak);
        return new Size(width,Math.Max(P(30),text.Height+P(8)));
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Obsidian.Background);
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f;
        var box=new RectangleF(2*s,(Height-18*s)/2,18*s,18*s);using var fill=new SolidBrush(Checked?Obsidian.Mint:Obsidian.Surface);e.Graphics.FillRectangle(fill,box);
        using var border=new Pen(Focused?Obsidian.Mint:Obsidian.Border,s);e.Graphics.DrawRectangle(border,box.X,box.Y,box.Width,box.Height);
        if(Checked){using var check=new Pen(Obsidian.Background,2*s);e.Graphics.DrawLines(check,[new PointF(box.X+4*s,box.Y+9*s),new(box.X+8*s,box.Y+13*s),new(box.X+15*s,box.Y+5*s)]);}
        TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle((int)(32*s),0,Math.Max(1,Width-(int)(32*s)),Height),Enabled?Obsidian.Text:Obsidian.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);
        if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-1,-1));
    }
}
