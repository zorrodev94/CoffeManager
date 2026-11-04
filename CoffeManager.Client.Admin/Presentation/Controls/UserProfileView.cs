using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Displays persisted account identity and every profile field without exposing authentication or subscription data.</summary>
internal sealed class UserProfileView : Panel
{
    private readonly ProfileHeader _header;
    private readonly ProfileInfoCard[] _cards;
    private bool _arranging;
    internal event EventHandler? EditRequested;
    internal UserProfileView(UserDetailsData user,Image? avatar,Image? cover)
    {
        BackColor=Obsidian.Background;DoubleBuffered=true;Margin=new Padding(0,12,0,12);
        _header=new ProfileHeader(user,avatar,cover);_header.EditRequested+=(_,e)=>EditRequested?.Invoke(this,e);Controls.Add(_header);
        string Value(string? value)=>string.IsNullOrWhiteSpace(value)?"Sin registrar":value;
        var p=user.Profile;
        _cards=[new("Presentación",[("Nombre para mostrar",Value(p.DisplayName)),("Descripción",Value(p.Description))]),
            new("Contacto",[("Correo electrónico",user.Email),("Teléfono",Value(p.PhoneNumber)),("Teléfono alternativo",Value(p.AlternatePhoneNumber))]),
            new("Ubicación",[("Calle, número y domicilio",Value(p.Address)),("Ciudad",Value(p.City)),("Estado",Value(p.State)),("Código postal",Value(p.PostalCode))]),
            new("Información personal",[("Fecha de nacimiento",p.DateOfBirth?.ToString("dd/MM/yyyy")??"Sin registrar"),("RFC",Value(p.TaxIdentificationNumber)),("CURP",Value(p.NationalIdentificationNumber))])];
        Controls.AddRange(_cards);
    }
    protected override void OnSizeChanged(EventArgs e){base.OnSizeChanged(e);ArrangeView();}
    internal void ArrangeView()
    {
        if(_arranging||_header is null||_cards is null)return;_arranging=true;
        try
        {
            int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);int gap=P(16),columns=Width>=P(720)?2:1,w=Math.Max(1,(Width-gap*(columns-1))/columns);
            _header.SetBounds(0,0,Width,P(Width<P(520)?370:290));int y=_header.Bottom+gap;
            for(int i=0;i<_cards.Length;i+=columns)
            {
                int height=0;for(int j=i;j<Math.Min(i+columns,_cards.Length);j++)height=Math.Max(height,_cards[j].RequiredHeight(w));
                for(int j=i;j<Math.Min(i+columns,_cards.Length);j++)_cards[j].SetBounds((j-i)*(w+gap),y,w,height);
                y+=height+gap;
            }
            Height=y;
        }
        finally{_arranging=false;}
    }
}
/// <summary>Renders a circular user image or initials; source images remain owned by the hosting page.</summary>
internal sealed class CircularAvatar : Control
{
    internal Image? Source;
    internal string Initials="CM";
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if(Width<1||Height<1)return;
        using var outline=new GraphicsPath();outline.AddEllipse(1,1,Math.Max(1,Width-2),Math.Max(1,Height-2));
        var previous=Region;Region=new Region(outline);previous?.Dispose();
    }
    internal CircularAvatar(){TabStop=false;SetStyle(ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);}
    protected override void OnPaint(PaintEventArgs e)
    {
        var g=e.Graphics;g.Clear(Parent?.BackColor??Obsidian.Surface);g.SmoothingMode=SmoothingMode.AntiAlias;
        var circle=new Rectangle(3,3,Math.Max(1,Width-6),Math.Max(1,Height-6));using var path=new GraphicsPath();path.AddEllipse(circle);var state=g.Save();g.SetClip(path);
        if(Source is not null)
        {
            float scale=Math.Max(circle.Width/(float)Source.Width,circle.Height/(float)Source.Height);float w=Source.Width*scale,h=Source.Height*scale;g.DrawImage(Source,circle.X+(circle.Width-w)/2,circle.Y+(circle.Height-h)/2,w,h);
        }
        else{using var fill=new SolidBrush(Obsidian.Mint);g.FillEllipse(fill,circle);using var font=new Font("Segoe UI Semibold",Math.Max(10,Height*72f/DeviceDpi*.24f));TextRenderer.DrawText(g,Initials,font,circle,Obsidian.Background,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
        g.Restore(state);using var border=new Pen(Obsidian.Mint,2*DeviceDpi/96f);g.DrawEllipse(border,circle);
    }
}
internal sealed class ProfileHeader : Panel
{
    private readonly CoverImagePanel _cover;
    private readonly CircularAvatar _avatar;
    private readonly Label _name,_role,_description;
    private readonly Button _edit,_changeCover;
    internal event EventHandler? EditRequested;
    internal ProfileHeader(UserDetailsData user,Image? avatar,Image? cover)
    {
        BackColor=Obsidian.Surface;DoubleBuffered=true;
        _cover=new CoverImagePanel{Fit=false,HorizontalFocus=0.5f,BackColor=Obsidian.Background,Source=cover??BrandResources.RefinedBackground,AccessibleName="Portada del perfil"};
        string initials=string.Concat(new[]{user.FirstName,user.PaternalLastName}.Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x.Trim()[0])).ToUpperInvariant();
        _avatar=new CircularAvatar{Source=avatar,Initials=initials,AccessibleName="Foto del perfil"};
        _name=Obsidian.Label(user.Profile.DisplayName,18,true);_role=Obsidian.Label(user.IsOwner?"Propietario":"Cuenta de acceso",10);_role.ForeColor=Obsidian.Mint;
        _description=Obsidian.Label(string.IsNullOrWhiteSpace(user.Profile.Description)?"Completa tu presentación.":user.Profile.Description,10);_description.ForeColor=Obsidian.Muted;
        _edit=Obsidian.Action("Editar perfil",(_,e)=>EditRequested?.Invoke(this,e));_changeCover=Obsidian.Action("Cambiar foto o portada",(_,e)=>EditRequested?.Invoke(this,e),false);
        Controls.AddRange([_cover,_avatar,_name,_role,_description,_edit,_changeCover]);
        _cover.SendToBack();_avatar.BringToFront();_changeCover.BringToFront();
        foreach(var label in new[]{_name,_role,_description}){label.AutoSize=false;label.BackColor=BackColor;label.AutoEllipsis=true;}
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_cover is null)return;int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);bool narrow=Width<P(520);
        _cover.SetBounds(0,0,Width,P(150));_avatar.SetBounds(P(22),P(106),P(106),P(106));
        _changeCover.SetBounds(Math.Max(P(12),Width-P(218)),P(12),Math.Min(P(202),Math.Max(1,Width-P(24))),P(36));
        int x=narrow?P(22):P(148),y=narrow?P(216):P(160),w=Math.Max(1,Width-x-P(narrow?20:200));
        _name.SetBounds(x,y,w,P(34));_role.SetBounds(x,y+P(36),w,P(24));_description.SetBounds(x,y+P(62),w,P(38));
        _edit.SetBounds(Math.Max(P(22),Width-P(180)),narrow?Height-P(54):P(166),P(158),P(38));
        _edit.BringToFront();
    }
}
internal sealed class ProfileInfoCard : Panel
{
    private readonly Label _heading;
    private readonly (Label Name,Label Value)[] _rows;
    internal ProfileInfoCard(string heading,(string Name,string Value)[] rows)
    {
        SuspendLayout();BackColor=Obsidian.Surface;DoubleBuffered=true;ResizeRedraw=true;_heading=Obsidian.Label(heading,14,true);_heading.AutoSize=false;Controls.Add(_heading);
        _rows=rows.Select(x=>(Obsidian.Label(x.Name,9),Obsidian.Label(x.Value,10))).ToArray();
        foreach(var row in _rows){row.Name.AutoSize=false;row.Name.ForeColor=Obsidian.Muted;row.Value.AutoSize=false;Controls.AddRange([row.Name,row.Value]);}
        foreach(Control child in Controls)child.BackColor=BackColor;
        ResumeLayout(true);
    }
    internal int RequiredHeight(int width)=>ArrangeRows(width,false);
    protected override void OnLayout(LayoutEventArgs e){base.OnLayout(e);if(_heading is not null&&_rows is not null)ArrangeRows(Width,true);}
    private int ArrangeRows(int width,bool apply)
    {
        int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);int inset=P(20),inner=Math.Max(1,width-inset*2),y=P(58);bool stack=inner<P(340);
        if(apply)_heading.SetBounds(inset,P(16),inner,P(30));
        foreach(var row in _rows)
        {
            int left=stack?inner:Math.Min(P(150),inner/2),right=stack?inner:Math.Max(1,inner-left-P(14));
            int nameH=TextRenderer.MeasureText(row.Name.Text,row.Name.Font,new Size(left,int.MaxValue),TextFormatFlags.WordBreak).Height+P(3);
            int valueH=TextRenderer.MeasureText(row.Value.Text,row.Value.Font,new Size(right,int.MaxValue),TextFormatFlags.WordBreak).Height+P(3);
            if(apply){row.Name.SetBounds(inset,y,left,nameH);row.Value.SetBounds(stack?inset:inset+left+P(14),stack?y+nameH:y,right,valueH);}
            y+=(stack?nameH+valueH:Math.Max(nameH,valueH))+P(12);
        }
        return y+P(8);
    }
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);using var border=new Pen(Obsidian.Border);e.Graphics.DrawRectangle(border,0,0,Width-1,Height-1);}
}
