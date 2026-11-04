using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Frames a native keyboard-accessible list with one dark surface and a local chevron.</summary>
internal sealed class ObsidianSelector : Panel
{
    #region Operation Data
        internal ComboBox Input { get; }
        private readonly Panel _clip;
        private readonly ChevronButton _arrow;
    #endregion Operation Data
    #region Initialization
        internal ObsidianSelector(string label)
        {
            BackColor=Obsidian.Surface;Margin=new Padding(0,0,0,14);Height=48;
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
            Input=new ObsidianComboBox{DropDownStyle=ComboBoxStyle.DropDownList,DrawMode=DrawMode.OwnerDrawFixed,FlatStyle=FlatStyle.Flat,BackColor=Obsidian.Surface,ForeColor=Obsidian.Text,Font=new Font("Segoe UI",11),AccessibleName=label,IntegralHeight=false,DropDownHeight=240};
            _clip=new Panel{BackColor=Obsidian.Surface,TabStop=false};_clip.Controls.Add(Input);
            _arrow=new ChevronButton{BackColor=Obsidian.Surface,AccessibleName="Abrir opciones de "+label,TabStop=false};
            _arrow.Click+=(_,_)=>{if(Input.Enabled){Input.Focus();Input.DroppedDown=!Input.DroppedDown;}};
            Input.DrawItem+=(_,e)=>
            {
                if(e.Index<0)return;var selected=(e.State&DrawItemState.Selected)!=0;
                using var fill=new SolidBrush(selected?Color.FromArgb(44,70,63):Obsidian.Surface);e.Graphics.FillRectangle(fill,e.Bounds);
                TextRenderer.DrawText(e.Graphics,Input.GetItemText(Input.Items[e.Index]),Input.Font,Rectangle.Inflate(e.Bounds,-6,0),!Input.Enabled?Obsidian.Muted:Obsidian.Text,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
            };
            Input.Enter+=(_,_)=>Invalidate();Input.Leave+=(_,_)=>Invalidate();Input.EnabledChanged+=(_,_)=>{_arrow.Enabled=Input.Enabled;Invalidate(true);};
            Controls.AddRange([_clip,_arrow]);
        }
    #endregion Initialization
    #region Layout And Drawing
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);if(Input is null)return;int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);
            Input.ItemHeight=Math.Max(P(26),Input.Font.Height+P(6));
            int editorHeight=Input.PreferredHeight;
            _clip.SetBounds(P(12),Math.Max(P(4),(Height-editorHeight)/2)+P(2),Math.Max(1,Width-P(58)),Math.Max(1,editorHeight-P(4)));
            Input.SetBounds(-P(2),-P(2),_clip.Width+P(44),editorHeight);
            Input.DropDownWidth=Math.Max(Input.Width,Width-P(24));
            _arrow.SetBounds(Math.Max(0,Width-P(42)),P(8),P(30),Math.Max(1,Height-P(16)));
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(Parent?.BackColor??Obsidian.Background);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),8*DeviceDpi/96f);using var fill=new SolidBrush(Obsidian.Surface);e.Graphics.FillPath(fill,path);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var path=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),8*DeviceDpi/96f);using var pen=new Pen(ContainsFocus?Obsidian.Mint:Obsidian.Border,DeviceDpi/96f);e.Graphics.DrawPath(pen,path);
        }
        private sealed class ChevronButton : Button
        {
            internal ChevronButton(){FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;Cursor=Cursors.Hand;SetStyle(ControlStyles.ResizeRedraw|ControlStyles.OptimizedDoubleBuffer,true);}
            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f,x=Width/2f,y=Height/2f;
                using var pen=new Pen(Enabled?Obsidian.Muted:Obsidian.Border,1.7f*s);e.Graphics.DrawLines(pen,[new PointF(x-5*s,y-2*s),new(x,y+3*s),new(x+5*s,y-2*s)]);
            }
        }
    #endregion Layout And Drawing
}
