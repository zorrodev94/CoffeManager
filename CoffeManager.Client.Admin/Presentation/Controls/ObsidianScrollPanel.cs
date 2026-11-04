using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Scrolls content with a local DPI-aware scrollbar instead of native light scroll tracks.</summary>
internal class ObsidianScrollPanel : Panel
{
    private readonly SlimScrollBar _bar;
    private int _extent,_offset;
    private Bitmap? _backdrop;
    private int _backdropDpi;
    internal event Action? ArrangeRequested;
    internal bool DecoratedBackground=true;
    internal IEnumerable<Control> ContentControls=>Controls.Cast<Control>().Where(x=>x!=_bar);
    internal int ScrollOffset=>_offset;
    internal int ContentHeight=>_extent;
    internal int ContentWidth=>Math.Max(1,ClientSize.Width-(int)(16*DeviceDpi/96f));
    internal ObsidianScrollPanel()
    {
        AutoScroll=false;DoubleBuffered=true;BackColor=Obsidian.Background;SetStyle(ControlStyles.ResizeRedraw,true);
        _bar=new SlimScrollBar();Controls.Add(_bar);_bar.ValueChanged+=value=>SetOffset(value);
    }
    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);if(e.Control is not null&&e.Control!=_bar)Watch(e.Control);
    }
    private void Watch(Control control)
    {
        if(control.TabStop&&control is not Panel)control.Enter+=(_,_)=>ScrollControlIntoView(control);
        foreach(Control child in control.Controls)Watch(child);
        control.ControlAdded+=(_,e)=>{if(e.Control is not null)Watch(e.Control);};
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_bar is null)return;_bar.SetBounds(Math.Max(0,Width-(int)(12*DeviceDpi/96f)),0,Math.Max(8,(int)(10*DeviceDpi/96f)),Height);_bar.BringToFront();ArrangeRequested?.Invoke();
    }
    internal void SetExtent(int extent)
    {
        int previous=_offset;_extent=Math.Max(0,extent);_offset=Math.Clamp(_offset,0,Math.Max(0,_extent-ClientSize.Height));
        if(previous!=_offset)foreach(var child in ContentControls)child.Top+=previous-_offset;
        if(_bar is null)return;_bar.Configure(_extent,ClientSize.Height,_offset);_bar.Visible=_extent>ClientSize.Height;_bar.BringToFront();
    }
    private void SetOffset(int value)
    {
        int next=Math.Clamp(value,0,Math.Max(0,_extent-ClientSize.Height));if(next==_offset)return;_offset=next;PerformLayout();Invalidate();
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        if(e is HandledMouseEventArgs handled)handled.Handled=true;
        SetOffset(_offset-(int)(e.Delta/120f*72*DeviceDpi/96f));
    }
    public new void ScrollControlIntoView(Control control)
    {
        if(!IsHandleCreated||control.IsDisposed||!control.IsHandleCreated)return;
        // A nested native editor is smaller than its visible field frame.
        for(Control? ancestor=control.Parent;ancestor is not null&&ancestor!=this;ancestor=ancestor.Parent)
            if(ancestor is ObsidianSelector or ObsidianField){control=ancestor;break;}
        var point=PointToClient(control.PointToScreen(Point.Empty));
        if(point.Y<0)SetOffset(_offset+point.Y-8);else if(point.Y+control.Height>ClientSize.Height)SetOffset(_offset+point.Y+control.Height-ClientSize.Height+8);
    }
    internal void ClearContent(){foreach(var child in ContentControls.ToArray())child.Dispose();SetOffset(0);SetExtent(0);}
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        if(!DecoratedBackground){base.OnPaintBackground(e);return;}
        if(Width<1||Height<1)return;
        // Native child editors can request the same background repeatedly during a scroll.
        if(_backdrop is null||_backdrop.Size!=ClientSize||_backdropDpi!=DeviceDpi)
        {
            _backdrop?.Dispose();_backdrop=new Bitmap(Width,Height);_backdropDpi=DeviceDpi;
            using var graphics=Graphics.FromImage(_backdrop);ObsidianBackdrop.Draw(graphics,ClientRectangle);
        }
        e.Graphics.DrawImageUnscaled(_backdrop,Point.Empty);
    }
    protected override void Dispose(bool disposing){if(disposing){_backdrop?.Dispose();_backdrop=null;}base.Dispose(disposing);}
    private sealed class SlimScrollBar : Control
    {
        private int _extent,_viewport,_value,_grab;
        private bool _drag;
        internal event Action<int>? ValueChanged;
        internal SlimScrollBar(){TabStop=true;AccessibleName="Desplazamiento vertical";AccessibleRole=AccessibleRole.ScrollBar;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
        internal void Configure(int extent,int viewport,int value){_extent=extent;_viewport=viewport;_value=value;Invalidate();}
        private Rectangle Thumb
        {
            get{int h=Math.Min(Height,Math.Max((int)(30*DeviceDpi/96f),(int)((long)Height*_viewport/Math.Max(1,_extent))));int y=(int)((long)Math.Max(0,Height-h)*_value/Math.Max(1,_extent-_viewport));return new Rectangle(2,y,Math.Max(3,Width-4),h);}
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Obsidian.Background);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            var thumb=Thumb;if(thumb.Width<1||thumb.Height<1)return;using var path=StudioSurface.Outline(thumb,thumb.Width/2f);using var brush=new SolidBrush(_drag||Focused?Obsidian.Mint:Color.FromArgb(83,113,105));e.Graphics.FillPath(brush,path);
        }
        protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();_grab=Thumb.Contains(e.Location)?e.Y-Thumb.Top:Thumb.Height/2;_drag=true;Capture=true;MoveThumb(e.Y);}
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(_drag)MoveThumb(e.Y);}
        private void MoveThumb(int y)=>ValueChanged?.Invoke((int)((long)Math.Clamp(y-_grab,0,Math.Max(1,Height-Thumb.Height))*Math.Max(0,_extent-_viewport)/Math.Max(1,Height-Thumb.Height)));
        protected override void OnMouseUp(MouseEventArgs e){_drag=false;Capture=false;Invalidate();base.OnMouseUp(e);}
        protected override void OnMouseCaptureChanged(EventArgs e){_drag=false;base.OnMouseCaptureChanged(e);Invalidate();}
        protected override void OnKeyDown(KeyEventArgs e)
        {
            int value=e.KeyCode switch{Keys.Down=>_value+40,Keys.Up=>_value-40,Keys.PageDown=>_value+_viewport,Keys.PageUp=>_value-_viewport,Keys.Home=>0,Keys.End=>_extent,_=>_value};ValueChanged?.Invoke(value);e.Handled=true;base.OnKeyDown(e);
        }
    }
}
/// <summary>Stacks administration content inside the dark scroll viewport.</summary>
internal sealed class ObsidianFlowPanel : ObsidianScrollPanel
{
    private bool _arranging;
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_arranging)return;_arranging=true;
        try
        {
            int width=Math.Max(1,ContentWidth-Padding.Horizontal),y=Padding.Top;
            foreach(Control child in ContentControls)
            {
                if(!child.Visible)continue;
                int h=child.Height;
                if(child is Label label){label.AutoSize=false;h=string.IsNullOrEmpty(label.Text)?0:TextRenderer.MeasureText(label.Text,label.Font,new Size(width,int.MaxValue),TextFormatFlags.WordBreak).Height+4;}
                if(child is HomeDashboard dashboard){dashboard.Width=width;dashboard.ArrangeDashboard();h=dashboard.Height;}
                else if(child is UserProfileView profile){profile.Width=width;profile.ArrangeView();h=profile.Height;}
                else if(child is FlowLayoutPanel card){card.MinimumSize=Size.Empty;card.MaximumSize=new Size(width,0);card.Width=width;h=card.GetPreferredSize(new Size(width,0)).Height;}
                int w=child is Button?Math.Min(child.Width,width):width;
                y+=child.Margin.Top;child.SetBounds(Padding.Left,y-ScrollOffset,w,h);y+=h+child.Margin.Bottom;
            }
            SetExtent(y+Padding.Bottom);
        }
        finally{_arranging=false;}
    }
}
