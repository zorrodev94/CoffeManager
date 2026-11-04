using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws an accessible dark-theme action with keyboard focus and pointer states.</summary>
internal sealed class ObsidianButton : Button
{
    #region Operation Data
        private readonly bool _primary;
        private bool _hovered;
    #endregion Operation Data
    #region Initialization
        internal ObsidianButton(bool primary)
        {
            _primary=primary;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;
            BackColor=Obsidian.Background;ForeColor=Obsidian.Text;Cursor=Cursors.Hand;Font=new Font("Segoe UI Semibold",11);
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        }
    #endregion Initialization
    #region Drawing Operations
        protected override void OnMouseEnter(EventArgs e){_hovered=true;Invalidate();base.OnMouseEnter(e);}
        protected override void OnMouseLeave(EventArgs e){_hovered=false;Invalidate();base.OnMouseLeave(e);}
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor??Obsidian.Background);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var outline=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),9*DeviceDpi/96f);
            var fill=!Enabled?Obsidian.Surface:_primary?(_hovered?Color.FromArgb(175,237,214):Obsidian.Mint):(_hovered?Color.FromArgb(37,46,50):Obsidian.Background);
            using var brush=new SolidBrush(fill);e.Graphics.FillPath(brush,outline);
            if(!_primary){using var pen=new Pen(Obsidian.Border);e.Graphics.DrawPath(pen,outline);}
            TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,!Enabled?Obsidian.Muted:_primary?Obsidian.Background:Obsidian.Text,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-7,-7),Obsidian.Text,fill);
        }
    #endregion Drawing Operations
}
