using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws a consistent rounded action with pointer, keyboard and disabled states.</summary>
internal sealed class StudioButton : Button
{
    #region Operation Data
        private bool _hovered;
        private bool _pressed;
        private readonly bool _primary;
    #endregion Operation Data
    #region Initialization
        internal StudioButton(bool primary)
        {
            _primary=primary;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;UseVisualStyleBackColor=false;
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
        }
    #endregion Initialization
    #region Drawing Operations
        protected override void OnMouseEnter(EventArgs e){_hovered=true;Invalidate();base.OnMouseEnter(e);}
        protected override void OnMouseLeave(EventArgs e){_hovered=false;_pressed=false;Invalidate();base.OnMouseLeave(e);}
        protected override void OnMouseDown(MouseEventArgs e){_pressed=true;Invalidate();base.OnMouseDown(e);}
        protected override void OnMouseUp(MouseEventArgs e){_pressed=false;Invalidate();base.OnMouseUp(e);}
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor??MarfilCafe.Ivory);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            var fill=!Enabled?Color.FromArgb(220,213,204):_primary?(_pressed?Color.FromArgb(139,66,22):_hovered?Color.FromArgb(194,108,40):MarfilCafe.Amber):(_hovered?Color.FromArgb(237,228,216):Color.White);
            using var path=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),10*DeviceDpi/96f);using var brush=new SolidBrush(fill);e.Graphics.FillPath(brush,path);
            if(!_primary){using var border=new Pen(Color.FromArgb(211,195,177));e.Graphics.DrawPath(border,path);}
            TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,!Enabled?Color.Gray:_primary?Color.White:MarfilCafe.Espresso,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,Rectangle.Inflate(ClientRectangle,-6,-6));
        }
    #endregion Drawing Operations
}
