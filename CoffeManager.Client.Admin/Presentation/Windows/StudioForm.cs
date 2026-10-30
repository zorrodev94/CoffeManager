using System.Runtime.InteropServices;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Controls;
namespace CoffeManager.Client.Admin.Presentation.Windows;
/// <summary>Provides uniform window chrome with native move/resize behavior and accessible caption actions.</summary>
internal class StudioForm : Form
{
    #region Operation Data
        protected readonly Panel Body;
        private readonly Panel _caption;
        private readonly Label _title;
    #endregion Operation Data
    #region Initialization
        protected StudioForm()
        {
            FormBorderStyle=FormBorderStyle.None;Padding=new Padding(1);BackColor=Color.FromArgb(218,207,193);
            AutoScaleDimensions=new SizeF(96,96);AutoScaleMode=AutoScaleMode.Dpi;Font=new Font("Segoe UI",11);
            Body=new Panel{Dock=DockStyle.Fill,BackColor=MarfilCafe.Ivory};
            _caption=new Panel{Dock=DockStyle.Top,Height=52,BackColor=MarfilCafe.Ivory};
            _title=new Label{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(20,0,0,0),ForeColor=MarfilCafe.Espresso,Font=new Font("Segoe UI Semibold",11)};
            var actions=new FlowLayoutPanel{Dock=DockStyle.Right,Width=144,FlowDirection=FlowDirection.LeftToRight,WrapContents=false};
            foreach(var item in new[]{("Minimizar","−"),("Maximizar o restaurar","□"),("Cerrar","×")})
            {
                var button=new CaptionActionButton(item.Item1){AccessibleName=item.Item1,Size=new Size(48,50),Margin=Padding.Empty,FlatStyle=FlatStyle.Flat,BackColor=MarfilCafe.Ivory,ForeColor=MarfilCafe.Espresso,TabStop=true};button.FlatAppearance.BorderSize=0;
                if(item.Item1=="Cerrar"){button.FlatAppearance.MouseOverBackColor=Color.FromArgb(190,54,39);button.Click+=(_,_)=>Close();}
                else if(item.Item1=="Minimizar")button.Click+=(_,_)=>WindowState=FormWindowState.Minimized;
                else button.Click+=(_,_)=>ToggleMaximize();
                actions.Controls.Add(button);
            }
            _caption.Controls.Add(_title);_caption.Controls.Add(actions);Controls.Add(Body);Controls.Add(_caption);
            TextChanged+=(_,_)=>_title.Text=Text;
            _title.MouseDown+=(_,e)=>{if(e.Button==MouseButtons.Left){ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}};
            _title.DoubleClick+=(_,_)=>ToggleMaximize();
            WindowLayout.Configure(this);
        }
    #endregion Initialization
    #region Window Operations
        private void ToggleMaximize(){WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;_caption.Invalidate(true);}
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);if(!OperatingSystem.IsWindows())return;
            var preference=2;DwmSetWindowAttribute(Handle,33,ref preference,sizeof(int));
        }
        protected override void WndProc(ref Message message)
        {
            if(message.Msg==0x84&&WindowState==FormWindowState.Normal)
            {
                base.WndProc(ref message);if((int)message.Result!=1)return;
                var point=PointToClient(new Point(unchecked((short)(long)message.LParam),unchecked((short)((long)message.LParam>>16))));var edge=Math.Max(5,DeviceDpi*6/96);
                bool left=point.X<edge,right=point.X>=Width-edge,top=point.Y<edge,bottom=point.Y>=Height-edge;
                if(left||right||top||bottom)message.Result=(IntPtr)(top?(left?13:right?14:12):bottom?(left?16:right?17:15):left?10:11);return;
            }
            if(message.Msg==0x24)
            {
                var info=Marshal.PtrToStructure<MinMaxInfo>(message.LParam);var screen=Screen.FromHandle(Handle);info.MaxPosition=new Point(screen.WorkingArea.Left-screen.Bounds.Left,screen.WorkingArea.Top-screen.Bounds.Top);info.MaxSize=screen.WorkingArea.Size;info.MinTrackSize=MinimumSize;Marshal.StructureToPtr(info,message.LParam,false);message.Result=IntPtr.Zero;return;
            }
            base.WndProc(ref message);
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct MinMaxInfo{public Point Reserved;public Size MaxSize;public Point MaxPosition;public Size MinTrackSize;public Size MaxTrackSize;}
        [DllImport("user32.dll")]private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]private static extern IntPtr SendMessage(IntPtr window,int message,IntPtr wParam,IntPtr lParam);
        [DllImport("dwmapi.dll")]private static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
    #endregion Window Operations
}
