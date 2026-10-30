using System.Drawing.Drawing2D;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Provides a lightweight rounded surface for fields and content cards.</summary>
internal sealed class StudioSurface : Panel
{
    #region Initialization
        internal StudioSurface()
        {
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
            Padding=new Padding(14,12,14,12);BackColor=Color.White;
        }
    #endregion Initialization
    #region Drawing Operations
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor??Color.White);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=Outline(new RectangleF(1,1,Width-3,Height-3),10*DeviceDpi/96f);using var fill=new SolidBrush(BackColor);e.Graphics.FillPath(fill,path);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=Outline(new RectangleF(1,1,Width-3,Height-3),10*DeviceDpi/96f);
            using var pen=new Pen(ContainsFocus?Color.FromArgb(177,91,28):Color.FromArgb(218,207,193),ContainsFocus?2:1);
            e.Graphics.DrawPath(pen,path);
        }
        internal static GraphicsPath Outline(RectangleF rect,float radius)
        {
            var path=new GraphicsPath();var d=Math.Min(radius*2,Math.Min(rect.Width,rect.Height));
            path.AddArc(rect.X,rect.Y,d,d,180,90);path.AddArc(rect.Right-d,rect.Y,d,d,270,90);path.AddArc(rect.Right-d,rect.Bottom-d,d,d,0,90);path.AddArc(rect.X,rect.Bottom-d,d,d,90,90);path.CloseFigure();return path;
        }
    #endregion Drawing Operations
}
