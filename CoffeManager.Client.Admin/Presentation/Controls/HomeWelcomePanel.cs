using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Uses the embedded theme illustration as a bounded decorative accent, never as navigation or a metric.</summary>
internal sealed class HomeWelcomePanel : Panel
{
    private readonly PictureBox _cup;
    private readonly Label _title,_note;
    internal HomeWelcomePanel()
    {
        BackColor=Obsidian.Surface;Height=120;Margin=new Padding(0,12,0,12);DoubleBuffered=true;
        _cup=new PictureBox{Image=BrandResources.HomeCup,SizeMode=PictureBoxSizeMode.Zoom,BackColor=Obsidian.Surface,TabStop=false,AccessibleName="Taza decorativa Obsidiana"};
        _title=Obsidian.Label("Tu espacio de administración",16,true);_note=Obsidian.Label("Personas, establecimientos y datos del negocio, en un solo lugar.",10);_note.ForeColor=Obsidian.Muted;
        foreach(var label in new[]{_title,_note}){label.AutoSize=false;label.BackColor=Obsidian.Surface;}
        Controls.AddRange([_title,_note,_cup]);
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_cup is null)return;int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);Height=P(120);bool show=Width>=P(600);_cup.Visible=show;
        _cup.SetBounds(Math.Max(0,Width-P(180)),0,P(176),Height);int w=Math.Max(1,Width-P(show?218:40));_title.SetBounds(P(20),P(22),w,P(34));_note.SetBounds(P(20),P(62),w,P(48));
    }
}
