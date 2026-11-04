using CoffeManager.Client.Admin.Presentation.Controls;
namespace CoffeManager.Client.Admin.Presentation.Theme;
/// <summary>Defines the access-window palette and local controls.</summary>
internal static class Obsidian
{
    #region Theme Data
        internal static readonly Color Background=Color.FromArgb(15,20,23);
        internal static readonly Color Surface=Color.FromArgb(24,30,34);
        internal static readonly Color Border=Color.FromArgb(65,76,80);
        internal static readonly Color Text=Color.FromArgb(242,244,242);
        internal static readonly Color Muted=Color.FromArgb(173,186,190);
        internal static readonly Color Mint=Color.FromArgb(145,221,195);
    #endregion Theme Data
    #region Control Factories
        internal static Label Label(string text,float size=10,bool bold=false)=>new()
        {
            Text=text,AutoSize=true,ForeColor=Text,BackColor=Background,
            Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(0,0,0,7)
        };
        internal static Button Action(string text,EventHandler handler,bool primary=true)
        {
            var button=new ObsidianButton(primary){Text=text,Height=50,Width=350,Margin=new Padding(0,8,0,4),AccessibleName=text};
            button.Click+=handler;return button;
        }
        internal static LinkLabel Link(string text,EventHandler handler)
        {
            var link=new LinkLabel{Text=text,AutoSize=true,LinkColor=Mint,ActiveLinkColor=Text,VisitedLinkColor=Mint,BackColor=Background,Font=new Font("Segoe UI",10),Margin=new Padding(0,4,0,12),AccessibleName=text};
            link.Click+=handler;return link;
        }
    #endregion Control Factories
}
