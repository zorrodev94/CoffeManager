using CoffeManager.Client.Admin.Presentation.Windows;
using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Forms;
internal sealed class AccessForm : StudioForm
{
    #region Operation Data
        private readonly AdminSession _session;
        private readonly FlowLayoutPanel _panel;
        private readonly TextBox _email;
        private readonly TextBox _password;
        private readonly Label _status;
        private bool _busy;
    #endregion Operation Data
    #region Initialization
        internal AccessForm(AdminSession session)
        {
            _session=session;Text="CoffeManager · Acceso";Size=new Size(1200,850);MinimumSize=new Size(800,650);
            StartPosition=FormStartPosition.CenterScreen;BackColor=MarfilCafe.Ivory;Font=new Font("Segoe UI",11);AutoScaleDimensions=new SizeF(96F,96F);AutoScaleMode=AutoScaleMode.Dpi;
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,55));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,45));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            _panel=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(30)};
            _panel.Controls.Add(new PictureBox{Image=BrandResources.Logo,SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(100,100)});
            _panel.Controls.Add(MarfilCafe.Text("CoffeManager",30,true));_panel.Controls.Add(MarfilCafe.Text("Tu negocio, en orden.",16));
            _panel.Controls.Add(MarfilCafe.Text("Correo electrónico"));_email=MarfilCafe.Input();_email.MaxLength=254;_email.AccessibleName="Correo electrónico";AddInputSurface(_email);
            _panel.Controls.Add(MarfilCafe.Text("Contraseña"));_password=MarfilCafe.Input();_password.MaxLength=128;_password.UseSystemPasswordChar=true;_password.AccessibleName="Contraseña";AddInputSurface(_password);
            var login=MarfilCafe.Action("Iniciar sesión",async(_,_)=>await ExecuteAsync(LoginAsync));AcceptButton=login;_panel.Controls.Add(login);
            _panel.Controls.Add(MarfilCafe.Action("Activar primera cuenta",async(_,_)=>await ExecuteAsync(RegisterAsync),false));
            _status=MarfilCafe.Text("Accede a tu espacio de administración",10);_panel.Controls.Add(_status);
            _panel.Resize+=(_,_)=>{var width=Math.Max(200,_panel.ClientSize.Width-_panel.Padding.Horizontal-30);foreach(Control child in _panel.Controls)if(child is StudioSurface)child.Width=width;_status.MaximumSize=new Size(width,0);};
            layout.Controls.Add(_panel,0,0);layout.Controls.Add(new CoverImagePanel{Dock=DockStyle.Fill,Source=BrandResources.Background,BackColor=MarfilCafe.Espresso},1,0);Body.Controls.Add(layout);
        }
    #endregion Initialization
    #region Field Operations
        private void AddInputSurface(TextBox input)
        {
            var surface=new StudioSurface{Width=420,Height=54,Margin=new Padding(0,0,0,16)};input.Dock=DockStyle.Fill;surface.Controls.Add(input);input.Enter+=(_,_)=>surface.Invalidate();input.Leave+=(_,_)=>surface.Invalidate();_panel.Controls.Add(surface);
        }
    #endregion Field Operations
    #region Account Operations
        /// <summary>
        /// Authenticates the account and opens its administration workspace.
        /// </summary>
        private async Task LoginAsync()
        {
            await _session.User.LoginAsync(new LoginData{Email=_email.Text.Trim(),Password=_password.Text});_password.Clear();
            Hide();using var form=new AdminForm(_session);form.ShowDialog(this);Show();
            try { await _session.User.LogoutAsync(); } catch(HttpRequestException) { } catch(InvalidOperationException) { } catch(TaskCanceledException) { }
        }
        /// <summary>
        /// Collects initial activation credentials and requests creation of the first owner.
        /// </summary>
        private async Task RegisterAsync()
        {
            using var editor=new EditorForm("Activa tu primera cuenta");var name=editor.AddField("Nombre");name.MaxLength=80;
            var last=editor.AddField("Primer apellido");last.MaxLength=80;var second=editor.AddField("Segundo apellido (opcional)");second.MaxLength=80;
            var email=editor.AddField("Correo electrónico");email.MaxLength=254;
            var password=editor.AddField("Contraseña (12 a 128 caracteres)");password.MaxLength=128;password.UseSystemPasswordChar=true;
            var confirm=editor.AddField("Confirmar contraseña");confirm.MaxLength=128;confirm.UseSystemPasswordChar=true;
            var key=editor.AddField("Clave de activación entregada por el proveedor");key.MaxLength=200;key.UseSystemPasswordChar=true;
            editor.AddNote("Esta operación solo está disponible antes de crear el primer dueño.");
            editor.ValidateBeforeConfirm(()=>
            {
                if(password.Text!=confirm.Text)throw new ValidationException("Las contraseñas no coinciden.");
                return new AccountRegistrationData{FirstName=name.Text.Trim(),PaternalLastName=last.Text.Trim(),MaternalLastName=Optional(second),Email=email.Text.Trim(),Password=password.Text,SetupKey=key.Text};
            });
            if(editor.ShowDialog(this)!=DialogResult.OK)return;
            try
            {
                if(password.Text!=confirm.Text)throw new ValidationException("Las contraseñas no coinciden.");
                await _session.User.RegisterInitialAsync(new AccountRegistrationData{FirstName=name.Text.Trim(),PaternalLastName=last.Text.Trim(),MaternalLastName=Optional(second),Email=email.Text.Trim(),Password=password.Text,SetupKey=key.Text});
                _email.Text=email.Text.Trim();MessageBox.Show(this,"Cuenta creada. Ya puedes iniciar sesión.","Registro completado");
            }
            finally {password.Clear();confirm.Clear();key.Clear();}
        }
        /// <summary>
        /// Executes one asynchronous access operation and displays expected errors.
        /// </summary>
        /// <param name="action">The action used by this operation.</param>
        private async Task ExecuteAsync(Func<Task> action)
        {
            if(_busy)return;_busy=true;_panel.Enabled=false;_status.Text="Conectando…";
            try {await action();_status.Text="Listo para iniciar sesión";}
            catch(Exception error) when(error is HttpRequestException or TaskCanceledException or InvalidOperationException or ValidationException)
            {_status.Text=error is HttpRequestException?"No se pudo conectar. Revisa API local y certificado HTTPS.":error.Message;}
            finally{_busy=false;_panel.Enabled=true;}
        }
        /// <summary>
        /// Optional for the current entity operation.
        /// </summary>
        /// <param name="input">The input used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        private static string? Optional(TextBox input)=>string.IsNullOrWhiteSpace(input.Text)?null:input.Text.Trim();
    #endregion Account Operations
}
