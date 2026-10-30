using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws a local vector navigation icon without emoji fonts or animation.</summary>
internal sealed class NavigationButton : Button
{
    #region Operation Data
        private readonly string _caption;
        private readonly string _icon;
        private bool _compact;
    #endregion Operation Data
    #region Initialization
        /// <summary>Creates a keyboard-accessible navigation action.</summary>
        /// <param name="caption">The accessible Spanish action name.</param>
        /// <param name="icon">The local icon key.</param>
        internal NavigationButton(string caption,string icon)
        {
            _caption=caption;_icon=icon;AccessibleName=caption;FlatStyle=FlatStyle.Flat;
            FlatAppearance.BorderSize=0;FlatAppearance.MouseOverBackColor=Color.FromArgb(88,60,43);
            BackColor=MarfilCafe.Espresso;ForeColor=Color.White;Cursor=Cursors.Hand;
            Margin=new Padding(0,3,0,3);SetStyle(ControlStyles.OptimizedDoubleBuffer,true);
            SetCompact(false);
        }
    #endregion Initialization
    #region Navigation Operations
        /// <summary>Changes rail layout immediately while preserving its accessible caption.</summary>
        /// <param name="compact">Whether the button should display only its icon.</param>
        internal void SetCompact(bool compact)
        {
            _compact=compact;var scale=DeviceDpi/96f;AutoSize=false;MinimumSize=Size.Empty;
            Size=new Size((int)((compact?48:203)*scale),(int)(48*scale));Invalidate();
        }
        /// <summary>Draws crisp icons and captions at the current display scale.</summary>
        /// <param name="e">The graphics surface and invalidated bounds.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
            var state=g.Save();var scale=DeviceDpi/96f;g.ScaleTransform(scale,scale);g.TranslateTransform(12,12);
            using var pen=new Pen(Enabled?Color.White:Color.Gray,1.7f){StartCap=LineCap.Round,EndCap=LineCap.Round,LineJoin=LineJoin.Round};
            switch(_icon)
            {
                case "menu":g.DrawLine(pen,2,5,22,5);g.DrawLine(pen,2,12,22,12);g.DrawLine(pen,2,19,22,19);break;
                case "home":g.DrawLines(pen,[new(2,11),new(12,2),new(22,11)]);g.DrawRectangle(pen,5,11,14,11);g.DrawRectangle(pen,10,15,4,7);break;
                case "person":g.DrawEllipse(pen,8,2,8,8);g.DrawArc(pen,3,12,18,18,180,180);break;
                case "business":g.DrawRectangle(pen,4,9,16,13);g.DrawRectangle(pen,2,3,20,6);g.DrawRectangle(pen,8,14,5,8);break;
                case "branches":g.DrawRectangle(pen,8,1,8,6);g.DrawLine(pen,12,7,12,12);g.DrawLine(pen,4,12,20,12);g.DrawLine(pen,4,12,4,16);g.DrawLine(pen,20,12,20,16);g.DrawRectangle(pen,1,16,6,6);g.DrawRectangle(pen,17,16,6,6);break;
                case "employees":g.DrawEllipse(pen,2,3,7,7);g.DrawEllipse(pen,15,3,7,7);g.DrawArc(pen,0,12,12,16,180,180);g.DrawArc(pen,12,12,12,16,180,180);break;
                case "logout":g.DrawLines(pen,[new(10,2),new(3,2),new(3,22),new(10,22)]);g.DrawLine(pen,9,12,22,12);g.DrawLines(pen,[new(17,7),new(22,12),new(17,17)]);break;
            }
            g.Restore(state);
            if(!_compact)TextRenderer.DrawText(g,_caption,Font,new Rectangle((int)(48*scale),0,Width-(int)(52*scale),Height),ForeColor,TextFormatFlags.VerticalCenter|TextFormatFlags.Left|TextFormatFlags.EndEllipsis);
        }
    #endregion Navigation Operations
}
