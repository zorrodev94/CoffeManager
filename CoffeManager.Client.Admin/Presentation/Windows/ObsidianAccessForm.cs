using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Windows;
/// <summary>Uses explicit DPI metrics for bounded access fields and artwork that remains present on narrow screens.</summary>
internal class ObsidianAccessForm : StudioForm
{
    #region Operation Data
        protected readonly AccessStackPanel Fields;
        private readonly CoverImagePanel _art;
        private bool _arranging;
        private readonly Size _startupLogicalSize;
        private readonly bool _showArtwork;
        private readonly Panel _viewport;
        private readonly Panel _footer;
        private readonly Panel _actions;
        private Button? _primaryAction,_secondaryAction;
        protected readonly Label ConnectionStatus;
        protected readonly LinkLabel RetryConnection;
    #endregion Operation Data
    #region Initialization
        protected ObsidianAccessForm(string title,int width=1160,int height=760,bool showArtwork=true)
        {
            _showArtwork=showArtwork;_startupLogicalSize=new Size(width,height);AutoScaleMode=AutoScaleMode.None;Text="CoffeManager · "+title;Size=new Size(width,height);MinimumSize=new Size(560,500);StartPosition=FormStartPosition.CenterScreen;
            BackColor=Obsidian.Border;Body.BackColor=Obsidian.Background;SetCaptionPalette(Obsidian.Background,Obsidian.Text);
            _footer=new Panel{Dock=DockStyle.Bottom,BackColor=Obsidian.Background};
            ConnectionStatus=Obsidian.Label("CoffeManager · Administración",9);ConnectionStatus.AutoSize=false;ConnectionStatus.ForeColor=Obsidian.Muted;
            RetryConnection=new LinkLabel{Text="Reintentar",AutoSize=false,LinkColor=Obsidian.Mint,ActiveLinkColor=Obsidian.Text,VisitedLinkColor=Obsidian.Mint,Visible=false,Font=new Font("Segoe UI",9),AccessibleName="Reintentar conexión"};
            _footer.Controls.Add(ConnectionStatus);_footer.Controls.Add(RetryConnection);
            _viewport=new Panel{Dock=DockStyle.Fill,BackColor=Obsidian.Background};Body.Controls.Add(_viewport);Body.Controls.Add(_footer);
            Fields=new AccessStackPanel{CenterVertically=height<850,MaximumLogicalWidth=height>=850?680:460};
            _art=new CoverImagePanel{Source=BrandResources.RefinedBackground,BackColor=Obsidian.Background,AccessibleName="Cafetería nocturna"};
            _actions=new Panel{BackColor=Obsidian.Background,Visible=false};_viewport.Controls.Add(_actions);
            _viewport.Controls.Add(Fields);_viewport.Controls.Add(_art);_art.Visible=showArtwork;
            Body.Resize+=(_,_)=>Arrange();Shown+=(_,_)=>{ApplyChromeScale();Arrange();BeginInvoke((Action)Arrange);};DpiChanged+=(_,_)=>BeginInvoke((Action)(()=>{ApplyChromeScale();Arrange();}));
        }
    #endregion Initialization
    #region Startup Layout
        /// <summary>Converts the requested logical startup size once, before fitting it to the monitor working area.</summary>
        protected override void OnLoad(EventArgs e)
        {
            if(!TopLevel){base.OnLoad(e);Arrange();return;}
            var scale=DeviceDpi/96f;var area=Screen.FromControl(Owner??this).WorkingArea;
            Size=AccessLayoutMetrics.StartupSize(_startupLogicalSize,area.Size,DeviceDpi);
            var center=Owner?.Bounds??area;
            Location=new Point(Math.Clamp(center.Left+(center.Width-Width)/2,area.Left,Math.Max(area.Left,area.Right-Width)),Math.Clamp(center.Top+(center.Height-Height)/2,area.Top,Math.Max(area.Top,area.Bottom-Height)));
            MinimumSize=new Size(Math.Min((int)(560*scale),area.Width-24),Math.Min((int)(480*scale),area.Height-24));
            base.OnLoad(e);Arrange();
        }
    #endregion Startup Layout
    #region Field Operations
        protected ObsidianField AddField(string label,string placeholder,int maximumLength,bool password=false)
        {
            Fields.Controls.Add(Obsidian.Label(label));var field=new ObsidianField(label,placeholder,maximumLength,password);Fields.Controls.Add(field);return field;
        }
        protected (ObsidianField First,ObsidianField Second) AddPair(string firstLabel,string firstPlaceholder,int firstMax,string secondLabel,string secondPlaceholder,int secondMax,bool password=false)
        {
            var pair=new AccessPairPanel(firstLabel,firstPlaceholder,firstMax,secondLabel,secondPlaceholder,secondMax,password);Fields.Controls.Add(pair);return(pair.First,pair.Second);
        }
        /// <summary>Keeps submission actions reachable while the form fields scroll independently.</summary>
        protected void AddFormActions(Button primary,Button secondary)
        {
            _primaryAction=primary;_secondaryAction=secondary;_actions.Controls.AddRange([primary,secondary]);_actions.Visible=true;AcceptButton=primary;CancelButton=secondary;Arrange();
        }
        protected void AddHeading(string title,string description)
        {
            Fields.Controls.Add(new AccessBrand());
            var heading=Obsidian.Label(title,24,true);heading.Margin=new Padding(0,0,0,10);Fields.Controls.Add(heading);
            var note=Obsidian.Label(description,11);note.Tag="access-subtitle";note.ForeColor=Obsidian.Muted;note.Margin=new Padding(0,0,0,22);Fields.Controls.Add(note);
        }
        private void Arrange()
        {
            if(_arranging||Body.ClientSize.Width<=0)return;_arranging=true;
            try
            {
                int P(int value)=>(int)Math.Round(value*DeviceDpi/96f);
                _footer.Height=P(34);ConnectionStatus.SetBounds(P(22),P(6),Math.Max(1,_footer.Width-P(150)),P(24));RetryConnection.SetBounds(Math.Max(0,_footer.Width-P(110)),P(6),P(90),P(24));
                var regions=AccessLayoutMetrics.Calculate(_viewport.ClientSize.Width,_viewport.ClientSize.Height,DeviceDpi);
                var fieldRegion=_showArtwork?regions.Fields:_viewport.ClientRectangle;
                if(_primaryAction is not null&&_secondaryAction is not null)
                {
                    var actions=AccessLayoutMetrics.WithActions(fieldRegion,DeviceDpi,Fields.MaximumLogicalWidth);
                    _actions.Bounds=actions.Actions;_primaryAction.Bounds=actions.Primary;_secondaryAction.Bounds=actions.Secondary;fieldRegion=actions.Fields;
                }
                Fields.Bounds=fieldRegion;_art.Bounds=regions.Artwork;
                Fields.PerformLayout();_art.Invalidate();
            }
            finally{_arranging=false;}
        }
    #endregion Field Operations
}
