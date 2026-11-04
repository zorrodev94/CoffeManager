using System.Drawing;
using CoffeManager.Client.Admin.Presentation.Theme;
foreach(var dpi in new[]{96,144,192})
foreach(var size in new[]{(560,500),(670,450),(1280,720),(1366,768),(1920,1080),(3840,2160)})
{
    var regions=AccessLayoutMetrics.Calculate(size.Item1,size.Item2,dpi);
    if(regions.Fields.Width<=0||regions.Fields.Height<=0||regions.Artwork.Width<=0||regions.Artwork.Height<=0)throw new Exception("Empty region");
    if(regions.Fields.IntersectsWith(regions.Artwork))throw new Exception("Overlapping regions");
    foreach(var rect in new[]{regions.Fields,regions.Artwork})if(rect.X<0||rect.Y<0||rect.Right>size.Item1||rect.Bottom>size.Item2)throw new Exception("Viewport overflow");
    if(regions.Fields.Width*regions.Fields.Height+regions.Artwork.Width*regions.Artwork.Height!=size.Item1*size.Item2)throw new Exception("Unassigned viewport");
}
var normal=AccessLayoutMetrics.StartupSize(new Size(1160,760),new Size(3840,2080),192);
if(normal!=new Size(2320,1520))throw new Exception("Startup size was not scaled from logical units");
var laptop=AccessLayoutMetrics.StartupSize(new Size(1160,760),new Size(1366,728),96);
if(laptop.Width!=1160||laptop.Height>704)throw new Exception("Startup exceeds laptop working area");
Console.WriteLine("PASS: access regions remain visible, inside viewport and non-overlapping for 18 viewport/DPI combinations. Startup scaling at 200% and laptop bounds verified. Does not execute WinForms.");
foreach(var dpi in new[]{96,120,144,192})
foreach(var width in new[]{240,360,510,640,780,1024,1600,2400})
foreach(var actions in new[]{2,3})
{
    var layout=DashboardLayoutMetrics.Calculate(width,dpi,actions);
    var boxes=layout.Totals.Concat(new[]{layout.Heading}).Concat(layout.Actions).Append(layout.Activity).ToArray();
    foreach(var box in boxes)if(box.X<0||box.Right>width||box.Width<=0||box.Bottom>layout.Height)throw new Exception("Home viewport overflow");
    for(int i=0;i<boxes.Length;i++)for(int j=i+1;j<boxes.Length;j++)if(boxes[i].IntersectsWith(boxes[j]))throw new Exception("Home card overlap");
}
Console.WriteLine("PASS: Home card bounds, non-overlap and responsive rows: 64 viewport/DPI/action combinations. Geometry only; Windows rendering remains pending.");
