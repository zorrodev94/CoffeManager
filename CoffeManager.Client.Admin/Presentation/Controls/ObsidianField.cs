using System.Drawing.Drawing2D;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Hosts a native text editor with a DPI-scaled surface and optional password visibility.</summary>
internal sealed class ObsidianField : Panel
{
    #region Operation Data
        internal TextBox Input { get; }
        private readonly Button? _visibility;
        private readonly bool _multiline;
    #endregion Operation Data
    #region Initialization
        internal ObsidianField(string label,string placeholder,int maximumLength,bool password=false,bool multiline=false)
        {
            _multiline=multiline;BackColor=Obsidian.Surface;Height=multiline?122:48;Width=380;Margin=new Padding(0,0,0,16);
            SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
            Input=new TextBox{BorderStyle=BorderStyle.None,BackColor=Obsidian.Surface,ForeColor=Obsidian.Text,Font=new Font("Segoe UI",11),PlaceholderText=placeholder,MaxLength=maximumLength,UseSystemPasswordChar=password,Multiline=multiline,ScrollBars=ScrollBars.None,AccessibleName=label};
            Controls.Add(Input);Input.Enter+=(_,_)=>Invalidate();Input.Leave+=(_,_)=>Invalidate();
            if(password)
            {
                _visibility=new PasswordVisibilityButton{Text=string.Empty,FlatStyle=FlatStyle.Flat,BackColor=Obsidian.Surface,ForeColor=Obsidian.Mint,Cursor=Cursors.Hand,AccessibleName="Mostrar "+label.ToLowerInvariant(),TabStop=true};
                _visibility.FlatAppearance.BorderSize=0;
                _visibility.Click+=(_,_)=>{Input.UseSystemPasswordChar=!Input.UseSystemPasswordChar;((PasswordVisibilityButton)_visibility).SetRevealed(!Input.UseSystemPasswordChar);_visibility.AccessibleName=(Input.UseSystemPasswordChar?"Mostrar ":"Ocultar ")+label.ToLowerInvariant();};
                Controls.Add(_visibility);
            }
        }
    #endregion Initialization
    #region Layout And Drawing
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if(Parent?.BackColor==Color.Transparent)
            {
                var state=e.Graphics.Save();try{e.Graphics.TranslateTransform(-Left,-Top);InvokePaintBackground(Parent,new PaintEventArgs(e.Graphics,new Rectangle(Left,Top,Width,Height)));}finally{e.Graphics.Restore(state);}
            }
            else e.Graphics.Clear(Parent?.BackColor??Obsidian.Background);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var outline=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),8*DeviceDpi/96f);
            using var brush=new SolidBrush(Obsidian.Surface);e.Graphics.FillPath(brush,outline);
        }
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);if(Input is null)return;var scale=DeviceDpi/96f;int inset=(int)(14*scale),action=_visibility is null?0:(int)(44*scale);
            Input.SetBounds(inset,_multiline?inset:Math.Max(2,(Height-Input.PreferredHeight)/2),Math.Max(1,Width-2*inset-action),_multiline?Math.Max(1,Height-2*inset):Input.PreferredHeight);
            _visibility?.SetBounds(Width-action-(int)(8*scale),(int)(8*scale),Math.Max(1,action-(int)(4*scale)),Math.Max(1,Height-(int)(16*scale)));
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
            using var outline=StudioSurface.Outline(new RectangleF(1,1,Width-3,Height-3),8*DeviceDpi/96f);
            using var pen=new Pen(ContainsFocus?Obsidian.Mint:Obsidian.Border,(ContainsFocus?1.5f:1f)*DeviceDpi/96f);e.Graphics.DrawPath(pen,outline);
        }
    #endregion Layout And Drawing
}
