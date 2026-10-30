using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Draws a caption action using local vector strokes at the current monitor DPI.</summary>
internal sealed class CaptionActionButton(string action) : Button
{
    #region Drawing Operations
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            var scale=DeviceDpi/96f;var x=Width/2f;var y=Height/2f;using var pen=new Pen(MarfilCafe.Espresso,1.4f*scale);
            if(action=="Minimizar")e.Graphics.DrawLine(pen,x-5*scale,y,x+5*scale,y);
            else if(action=="Cerrar"){e.Graphics.DrawLine(pen,x-4*scale,y-4*scale,x+4*scale,y+4*scale);e.Graphics.DrawLine(pen,x+4*scale,y-4*scale,x-4*scale,y+4*scale);}
            else
            {
                if(FindForm()?.WindowState==FormWindowState.Maximized){e.Graphics.DrawRectangle(pen,x-3*scale,y-5*scale,8*scale,8*scale);using var brush=new SolidBrush(BackColor);e.Graphics.FillRectangle(brush,x-5*scale,y-3*scale,8*scale,8*scale);e.Graphics.DrawRectangle(pen,x-5*scale,y-3*scale,8*scale,8*scale);}
                else e.Graphics.DrawRectangle(pen,x-5*scale,y-5*scale,10*scale,10*scale);
            }
        }
    #endregion Drawing Operations
}
