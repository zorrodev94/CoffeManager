using System.Drawing.Drawing2D;
using System.ComponentModel;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws an image proportionally to fill a viewport without stretching its subject.</summary>
internal sealed class CoverImagePanel : Panel
{
    #region Operation Data
        internal bool Fit;
        internal float HorizontalFocus=0.8f;
        private Image? _source;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal Image? Source { get => _source; set { _source=value; Invalidate(); } }
    #endregion Operation Data
    #region Initialization
        internal CoverImagePanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    #endregion Initialization
    #region Drawing Operations
        /// <summary>Draws a cover crop biased toward the coffee subject on the right.</summary>
        /// <param name="e">The paint event data for the current viewport.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if(Source is null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            var scale = Fit ? Math.Min((float)ClientSize.Width / Source.Width, (float)ClientSize.Height / Source.Height) : Math.Max((float)ClientSize.Width / Source.Width, (float)ClientSize.Height / Source.Height);
            var width = Source.Width * scale;
            var height = Source.Height * scale;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(Source, (ClientSize.Width - width) * (Fit ? 0.5f : HorizontalFocus), (ClientSize.Height - height) * 0.5f, width, height);
        }
    #endregion Drawing Operations
}
