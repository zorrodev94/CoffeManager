using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Displays a local neutral avatar while no personal image is configured.</summary>
internal sealed class DefaultAvatar : Control
{
    #region Initialization
        internal DefaultAvatar()
        {
            Size=new Size(76,76);AccessibleName="Avatar predeterminado";
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
        }
    #endregion Initialization
    #region Drawing Operations
        /// <summary>Draws a neutral person symbol without requesting media from the server.</summary>
        /// <param name="e">The target graphics surface.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
            using var background=new SolidBrush(MarfilCafe.Ivory);using var silhouette=new SolidBrush(MarfilCafe.Muted);
            var size=Math.Min(Width,Height)-2;g.FillEllipse(background,1,1,size,size);
            g.FillEllipse(silhouette,size*.36f,size*.19f,size*.28f,size*.28f);
            g.FillEllipse(silhouette,size*.23f,size*.52f,size*.54f,size*.30f);
        }
    #endregion Drawing Operations
}
