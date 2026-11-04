using System.Drawing;
namespace CoffeManager.Client.Admin.Presentation.Theme;
/// <summary>Calculates responsive Home card bounds in physical pixels from the current viewport DPI.</summary>
internal static class DashboardLayoutMetrics
{
    internal sealed record Layout(Rectangle[] Totals,Rectangle Heading,Rectangle[] Actions,Rectangle Activity,int Height);
    internal static Layout Calculate(int width,int dpi,int actions)
    {
        width=Math.Max(1,width);int P(int n)=>(int)Math.Round(n*dpi/96f);
        int gap=P(16),columns=width>=P(780)?3:width>=P(510)?2:1;
        int cardWidth=Math.Max(1,(width-gap*(columns-1))/columns);
        var totals=Enumerable.Range(0,3).Select(i=>new Rectangle((i%columns)*(cardWidth+gap),(i/columns)*P(116),cardWidth,P(100))).ToArray();
        int y=((3+columns-1)/columns)*P(116)+P(16);var heading=new Rectangle(0,y,width,P(36));y+=P(48);
        var buttons=Enumerable.Range(0,actions).Select(i=>new Rectangle((i%columns)*(cardWidth+gap),y+(i/columns)*P(110),cardWidth,P(94))).ToArray();
        y+=((actions+columns-1)/columns)*P(110)+P(20);var activity=new Rectangle(0,y,width,P(194));
        return new(totals,heading,buttons,activity,activity.Bottom+P(16));
    }
}
