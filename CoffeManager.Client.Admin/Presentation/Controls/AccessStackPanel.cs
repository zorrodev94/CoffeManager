using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Arranges access fields in device pixels with a bounded column and vertical-only scrolling.</summary>
internal sealed class AccessStackPanel : ObsidianScrollPanel
{
    #region Operation Data
        private bool _arranging;
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal bool CenterVertically { get; set; }
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        internal int MaximumLogicalWidth { get; set; }=520;
    #endregion Operation Data
    #region Initialization
        internal AccessStackPanel(){AutoScroll=false;BackColor=Obsidian.Background;DoubleBuffered=true;SetStyle(ControlStyles.ResizeRedraw,true);}
    #endregion Initialization
    #region Layout Operations
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if(e.Control is Label)e.Control.TextChanged+=(_,_)=>PerformLayout();
            if(e.Control is not null)e.Control.VisibleChanged+=(_,_)=>PerformLayout();
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);if(_arranging||ClientSize.Width<=0)return;_arranging=true;
            try
            {
                int P(int value)=>(int)Math.Round(value*DeviceDpi/96f);
                int inset=P(ClientSize.Width<P(480)?18:32);
                bool compact=ClientSize.Height<P(580);
                int available=Math.Max(1,ContentWidth-inset*2);
                int width=Math.Min(P(MaximumLogicalWidth),available),height=P(compact?8:24);
                var sizes=new List<(Control Control,int Height,int Before,int After)>();
                foreach(Control control in ContentControls)
                {
                    if(Equals(control.Tag,"access-subtitle"))control.Visible=!compact;
                    if(!control.Visible)continue;
                    int h;
                    if(control is AccessPairPanel pair)h=pair.RequiredHeight(width);
                    else if(control is Label label){label.AutoSize=false;label.MaximumSize=Size.Empty;h=string.IsNullOrWhiteSpace(label.Text)?0:TextRenderer.MeasureText(label.Text,label.Font,new Size(width,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl).Height+P(2);}
                    else if(control is ObsidianField field)h=P(field.Input.Multiline?122:compact?40:48);
                    else if(control is ObsidianSelector)h=P(compact?40:48);
                    else if(control is AccessBrand)h=P(36);
                    else if(control is Button)h=P(compact?40:48);
                    else if(control is ComboBox combo)h=combo.PreferredHeight;
                    else if(control is CheckBox check)h=check.GetPreferredSize(new Size(width,0)).Height;
                    else h=P(50);
                    int before=P(compact?Math.Min(control.Margin.Top,4):control.Margin.Top),after=P(compact?Math.Min(control.Margin.Bottom,4):control.Margin.Bottom);
                    if(h==0){before=0;after=0;}
                    sizes.Add((control,h,before,after));height+=h+before+after;
                }
                height+=P(compact?8:24);SetExtent(height);
                int x=Math.Max(inset,(ContentWidth-width)/2);
                int y=(CenterVertically?Math.Max(P(compact?8:24),(ClientSize.Height-height)/2):P(compact?8:24))-ScrollOffset;
                foreach(var item in sizes){y+=item.Before;item.Control.SetBounds(x,y,width,item.Height);y+=item.Height+item.After;}

            }
            finally{_arranging=false;}
        }
    #endregion Layout Operations
}
/// <summary>Switches a pair of labelled fields between columns and rows at a logical-width breakpoint.</summary>
internal sealed class AccessPairPanel : Panel
{
    private readonly Label _firstLabel,_secondLabel;
    internal ObsidianField First { get; }
    internal ObsidianField Second { get; }
    internal AccessPairPanel(string firstLabel,string firstPlaceholder,int firstMax,string secondLabel,string secondPlaceholder,int secondMax,bool password)
    {
        BackColor=Color.Transparent;Margin=new Padding(0,0,0,12);SetStyle(ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
        _firstLabel=Obsidian.Label(firstLabel);_secondLabel=Obsidian.Label(secondLabel);
        First=new(firstLabel,firstPlaceholder,firstMax,password);Second=new(secondLabel,secondPlaceholder,secondMax,password);
        Controls.AddRange([_firstLabel,First,_secondLabel,Second]);
    }
    internal int RequiredHeight(int width)=>(int)Math.Round((width>=460*DeviceDpi/96f?88:184)*DeviceDpi/96f);
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(First is null)return;int P(int value)=>(int)Math.Round(value*DeviceDpi/96f);
        bool columns=Width>=P(460);int width=columns?(Width-P(16))/2:Width;
        _firstLabel.AutoSize=false;_secondLabel.AutoSize=false;
        _firstLabel.SetBounds(0,0,width,P(28));First.SetBounds(0,P(32),width,P(52));
        _secondLabel.SetBounds(columns?width+P(16):0,columns?0:P(96),width,P(28));Second.SetBounds(columns?width+P(16):0,columns?P(32):P(128),width,P(52));
    }
}
