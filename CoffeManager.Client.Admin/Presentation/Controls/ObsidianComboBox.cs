using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Keeps ComboBox data binding and keyboard selection while replacing its native light popup.</summary>
internal sealed class ObsidianComboBox : ComboBox
{
    private ToolStripDropDown? _popup;
    private readonly HashSet<ToolStripDropDown> _retired=[];
    internal ToolStripDropDown? OptionsPopup=>_popup;
    protected override void WndProc(ref Message m)
    {
        // CB_SHOWDROPDOWN and mouse opening use the same local dark list.
        if((m.Msg==0x014F&&m.WParam!=IntPtr.Zero)||m.Msg==0x0201){if(Enabled)OpenOptions();m.Result=IntPtr.Zero;return;}
        base.WndProc(ref m);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if(e.KeyCode==Keys.F4||(e.Alt&&e.KeyCode==Keys.Down)){OpenOptions();e.Handled=true;e.SuppressKeyPress=true;return;}base.OnKeyDown(e);
    }
    internal void OpenOptions()
    {
        if(_popup is not null){_popup.Close();return;}if(Items.Count==0||!IsHandleCreated)return;
        for(Control? ancestor=Parent;ancestor is not null;ancestor=ancestor.Parent)
            if(ancestor is ObsidianScrollPanel viewport){viewport.ScrollControlIntoView(this);break;}
        int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);
        var anchor=Parent is Panel clip?clip.Parent??Parent:Parent;var bounds=anchor?.RectangleToScreen(anchor.ClientRectangle)??RectangleToScreen(ClientRectangle);
        var area=Screen.FromControl(this).WorkingArea;int width=Math.Max(P(100),Math.Min(bounds.Width,area.Width-P(12))),rowHeight=Math.Max(P(38),Font.Height+P(12));
        int room=Math.Max(area.Bottom-bounds.Bottom,bounds.Top-area.Top)-P(8);int height=Math.Min(Math.Min(P(280),Math.Max(rowHeight,room)),Items.Count*rowHeight+P(8));
        var list=new ObsidianScrollPanel{DecoratedBackground=false,BackColor=Obsidian.Surface,Size=new Size(width,height),TabStop=false};var rows=new List<Button>();
        for(int i=0;i<Items.Count;i++)
        {
            int index=i;var row=new Button{Text=GetItemText(Items[i]),AccessibleName=GetItemText(Items[i]),AccessibleRole=AccessibleRole.ListItem,Font=Font,TextAlign=ContentAlignment.MiddleLeft,FlatStyle=FlatStyle.Flat,BackColor=i==SelectedIndex?Color.FromArgb(44,70,63):Obsidian.Surface,ForeColor=Obsidian.Text,Padding=new Padding(P(10),0,0,0),TabIndex=i};
            row.FlatAppearance.BorderSize=0;row.FlatAppearance.MouseOverBackColor=Color.FromArgb(44,70,63);row.Click+=(_,_)=>{SelectedIndex=index;_popup?.Close();};
            row.PreviewKeyDown+=(_,e)=>{if(e.KeyCode is Keys.Up or Keys.Down)e.IsInputKey=true;};
            row.KeyDown+=(_,e)=>{if(e.KeyCode is Keys.Up or Keys.Down){rows[Math.Clamp(index+(e.KeyCode==Keys.Down?1:-1),0,rows.Count-1)].Focus();e.Handled=true;}else if(e.KeyCode==Keys.Escape){_popup?.Close();e.Handled=true;}};
            rows.Add(row);list.Controls.Add(row);
        }
        void Arrange(){for(int i=0;i<rows.Count;i++)rows[i].SetBounds(0,P(4)+i*rowHeight-list.ScrollOffset,list.ContentWidth,rowHeight);list.SetExtent(Items.Count*rowHeight+P(8));}
        list.ArrangeRequested+=Arrange;Arrange();
        var host=new ToolStripControlHost(list){AutoSize=false,Size=list.Size,Margin=Padding.Empty,Padding=Padding.Empty};
        var popup=new ToolStripDropDown{AutoSize=true,BackColor=Obsidian.Border,Padding=new Padding(1),Margin=Padding.Empty,DropShadowEnabled=false};popup.Items.Add(host);_popup=popup;
        popup.Closed+=(_,_)=>
        {
            if(ReferenceEquals(_popup,popup))_popup=null;
            _retired.Add(popup);
            // Closed runs inside ToolStripDropDown.SetVisibleCore. Dispose only after it returns.
            if(!IsDisposed&&!Disposing&&IsHandleCreated)BeginInvoke((Action)(()=>
            {
                if(_retired.Remove(popup))popup.Dispose();
                if(!IsDisposed&&!Disposing&&IsHandleCreated&&_popup is null)Focus();
            }));
        };
        int y=bounds.Bottom+P(2);if(y+height>area.Bottom)y=Math.Max(area.Top,bounds.Top-height-P(2));popup.Show(new Point(Math.Clamp(bounds.Left,area.Left,Math.Max(area.Left,area.Right-width-P(2))),y));
        rows[Math.Clamp(SelectedIndex,0,rows.Count-1)].Focus();
    }
    protected override void Dispose(bool disposing){if(disposing){var popup=_popup;_popup=null;popup?.Dispose();foreach(var retired in _retired.ToArray())retired.Dispose();_retired.Clear();}base.Dispose(disposing);}
}
