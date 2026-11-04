using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Displays persisted totals and entry points in an adaptive Obsidian workbench without decorative photographs.</summary>
internal sealed class HomeDashboard : Panel
{
    #region Operation Data
        private readonly DashboardTile[] _totals;
        private readonly DashboardTile[] _actions;
        private readonly Label _actionsTitle,_activityTitle,_emptyTitle,_emptyDescription;
        private readonly Panel _activity;
        private readonly Panel _guide;
        private readonly Label _guideTitle,_guideNote;
        private readonly Button _profile;
        internal event EventHandler? OpenBusiness,OpenBranches,OpenEmployees,AddEmployee,RegisterAccount,OpenProfile;
        private bool _arranging;
    #endregion Operation Data
    #region Initialization
        internal HomeDashboard(string business,int branches,int employees,bool owner)
        {
            DoubleBuffered=true;BackColor=Obsidian.Background;Margin=new Padding(0,16,0,0);
            _totals=[new(business,"Mi negocio","business"),new(branches.ToString(),"Sucursales","branches"),new(employees.ToString(),"Empleados","employees")];
            _totals[0].Click+=(_,e)=>OpenBusiness?.Invoke(this,e);_totals[1].Click+=(_,e)=>OpenBranches?.Invoke(this,e);_totals[2].Click+=(_,e)=>OpenEmployees?.Invoke(this,e);
            _actions=owner?[new("Agregar empleado","Alta mínima de tu equipo","person"),new("Gestionar sucursales","Consulta tus establecimientos","branches"),new("Registrar cuenta","Acceso autorizado por el dueño","account")]:[new("Agregar empleado","Alta mínima de tu equipo","person"),new("Gestionar sucursales","Consulta tus establecimientos","branches")];
            _actions[0].Click+=(_,e)=>AddEmployee?.Invoke(this,e);_actions[1].Click+=(_,e)=>OpenBranches?.Invoke(this,e);if(owner)_actions[2].Click+=(_,e)=>RegisterAccount?.Invoke(this,e);
            _actionsTitle=Heading("Acciones rápidas");_activityTitle=Heading("Actividad del sistema");
            _activity=new Panel{BackColor=Obsidian.Surface};
            _emptyTitle=Obsidian.Label("Todo listo para comenzar",13,true);_emptyDescription=Obsidian.Label("El historial de actividad estará disponible en una próxima entrega.",10);_emptyDescription.ForeColor=Obsidian.Muted;
            _guide=new Panel{BackColor=Obsidian.Surface};_guideTitle=Obsidian.Label("Tu siguiente paso",13,true);_guideNote=Obsidian.Label("Personaliza tu foto y completa tus datos de contacto.",10);_guideNote.ForeColor=Obsidian.Muted;
            _profile=Obsidian.Action("Ir a mi perfil",(_,e)=>OpenProfile?.Invoke(this,e));_guide.Controls.AddRange([_guideTitle,_guideNote,_profile]);_activity.Controls.Add(_guide);
            _activityTitle.BackColor=Obsidian.Surface;
            _activity.Controls.AddRange([_activityTitle,_emptyTitle,_emptyDescription]);Controls.AddRange(_totals);Controls.Add(_actionsTitle);Controls.AddRange(_actions);Controls.Add(_activity);
            _activity.Paint+=(_,e)=>{using var border=new Pen(Obsidian.Border);e.Graphics.DrawRectangle(border,0,0,_activity.Width-1,_activity.Height-1);};
        }
    #endregion Initialization
    #region Layout Operations
        private static Label Heading(string text)=>Obsidian.Label(text,15,true);
        protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);ArrangeDashboard();}
        /// <summary>Reflows cards by available logical width rather than monitor dimensions.</summary>
        internal void ArrangeDashboard()
        {
            if(_arranging||_totals is null)return;_arranging=true;
            try
            {
                int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);
                var layout=DashboardLayoutMetrics.Calculate(Width,DeviceDpi,_actions.Length);
                for(int i=0;i<_totals.Length;i++)_totals[i].Bounds=layout.Totals[i];
                _actionsTitle.AutoSize=false;_actionsTitle.Bounds=layout.Heading;
                for(int i=0;i<_actions.Length;i++)_actions[i].Bounds=layout.Actions[i];
                _activity.Bounds=layout.Activity;
                bool guide=Width>=P(760);_guide.Visible=guide;int activityWidth=guide?(int)(Width*.62f):Width;
                _activityTitle.AutoSize=false;_activityTitle.SetBounds(P(22),P(18),Math.Max(1,activityWidth-P(44)),P(34));
                _emptyTitle.AutoSize=false;_emptyTitle.TextAlign=ContentAlignment.MiddleCenter;_emptyTitle.BackColor=Obsidian.Surface;_emptyTitle.SetBounds(P(18),P(78),Math.Max(1,activityWidth-P(36)),P(32));
                _emptyDescription.AutoSize=false;_emptyDescription.TextAlign=ContentAlignment.TopCenter;_emptyDescription.BackColor=Obsidian.Surface;_emptyDescription.SetBounds(P(18),P(118),Math.Max(1,activityWidth-P(36)),P(60));
                _guide.SetBounds(activityWidth,P(12),Math.Max(1,Width-activityWidth-P(16)),P(170));
                foreach(var label in new[]{_guideTitle,_guideNote}){label.AutoSize=false;label.BackColor=Obsidian.Surface;}
                _guideTitle.SetBounds(P(16),P(10),Math.Max(1,_guide.Width-P(32)),P(32));_guideNote.SetBounds(P(16),P(50),Math.Max(1,_guide.Width-P(32)),P(58));_profile.SetBounds(P(16),P(118),Math.Max(1,_guide.Width-P(32)),P(40));
                Height=layout.Height;
            }
            finally{_arranging=false;}
        }
    #endregion Layout Operations
}
/// <summary>Draws a local, keyboard-accessible card with a restrained geometric accent and vector icon.</summary>
internal sealed class DashboardTile : Button
{
    private readonly string _title,_subtitle,_icon;
    private bool _hover;
    internal DashboardTile(string title,string subtitle,string icon)
    {
        _title=title;_subtitle=subtitle;_icon=icon;AccessibleName=subtitle+": "+title;Text=string.Empty;Cursor=Cursors.Hand;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;BackColor=Obsidian.Surface;ForeColor=Obsidian.Text;Margin=Padding.Empty;
        SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);
        MouseEnter+=(_,_)=>{_hover=true;Invalidate();};MouseLeave+=(_,_)=>{_hover=false;Invalidate();};
    }
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Obsidian.Background);float s=DeviceDpi/96f;
        var box=new RectangleF(1,1,Width-3,Height-3);float radius=10*s;
        using var path=new GraphicsPath();path.AddArc(box.Left,box.Top,radius,radius,180,90);path.AddArc(box.Right-radius,box.Top,radius,radius,270,90);path.AddArc(box.Right-radius,box.Bottom-radius,radius,radius,0,90);path.AddArc(box.Left,box.Bottom-radius,radius,radius,90,90);path.CloseFigure();
        using var fill=new SolidBrush(_hover?Color.FromArgb(32,44,47):Obsidian.Surface);g.FillPath(fill,path);using var border=new Pen(Focused||_hover?Obsidian.Mint:Obsidian.Border,s);g.DrawPath(border,path);
        var state=g.Save();g.TranslateTransform(20*s,25*s);g.ScaleTransform(s,s);
        using var pen=new Pen(Obsidian.Mint,1.8f){LineJoin=LineJoin.Round,StartCap=LineCap.Round,EndCap=LineCap.Round};
        switch(_icon)
        {
            case "business":g.DrawRectangle(pen,1,8,28,21);g.DrawLines(pen,[new(0,8),new(4,0),new(26,0),new(30,8)]);g.DrawLine(pen,10,0,10,8);g.DrawLine(pen,20,0,20,8);g.DrawRectangle(pen,7,17,8,12);break;
            case "branches":g.DrawRectangle(pen,10,0,10,8);g.DrawLine(pen,15,8,15,16);g.DrawLine(pen,5,16,25,16);g.DrawLine(pen,5,16,5,21);g.DrawLine(pen,25,16,25,21);g.DrawRectangle(pen,0,21,10,8);g.DrawRectangle(pen,20,21,10,8);break;
            case "account":g.DrawRectangle(pen,4,0,22,29);g.DrawEllipse(pen,11,5,8,8);g.DrawArc(pen,8,15,14,12,180,180);g.DrawLine(pen,9,25,21,25);break;
            default:g.DrawEllipse(pen,6,0,10,10);g.DrawArc(pen,0,14,22,24,180,180);if(_icon=="employees"){g.DrawArc(pen,20,2,8,8,260,240);g.DrawArc(pen,22,15,9,16,270,90);}else{g.DrawLine(pen,26,12,26,24);g.DrawLine(pen,20,18,32,18);}break;
        }
        g.Restore(state);
        int left=(int)(68*s),w=Math.Max(1,Width-left-(int)(12*s));using var titleFont=new Font("Segoe UI",12,FontStyle.Bold);using var subFont=new Font("Segoe UI",9);
        TextRenderer.DrawText(g,_title,titleFont,new Rectangle(left,(int)(22*s),w,(int)(28*s)),Obsidian.Text,TextFormatFlags.EndEllipsis|TextFormatFlags.SingleLine);
        TextRenderer.DrawText(g,_subtitle,subFont,new Rectangle(left,(int)(53*s),w,(int)(34*s)),Obsidian.Muted,TextFormatFlags.WordBreak|TextFormatFlags.EndEllipsis);
    }
}
