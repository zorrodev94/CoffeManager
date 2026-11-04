using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Coordinates persistent one-time activation, login and owner-authorized staff registration.</summary>
internal sealed class AccessForm : ObsidianAccessForm
{
    #region Operation Data
        private readonly AdminSession _session;
        private readonly ObsidianField _email,_password;
        private readonly Button _activation,_staff;
        private readonly Label _errors;
        private bool _busy;
    #endregion Operation Data
    #region Initialization
        internal AccessForm(AdminSession session):base("Acceso")
        {
            _session=session;AddHeading("Bienvenido de nuevo","Accede a la administración de tu negocio.");
            _email=AddField("Correo electrónico","ejemplo@correo.com",254);
            _password=AddField("Contraseña","Ingresa tu contraseña",128,true);
            _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);Fields.Controls.Add(_errors);
            var login=Obsidian.Action("Iniciar sesión",async(_,_)=>await ExecuteAsync(LoginAsync));Fields.Controls.Add(login);AcceptButton=login;
            _activation=Obsidian.Action("Activar primera cuenta",async(_,_)=>await ExecuteAsync(RegisterAsync),false);_activation.Visible=false;Fields.Controls.Add(_activation);
            _staff=Obsidian.Action("Crear cuenta de empleado",async(_,_)=>await ExecuteAsync(RegisterStaffAsync),false);_staff.Visible=false;Fields.Controls.Add(_staff);
            var note=Obsidian.Label("Las nuevas cuentas requieren autorización del propietario.",9);note.ForeColor=Obsidian.Muted;Fields.Controls.Add(note);
            RetryConnection.LinkClicked+=async(_,_)=>await ExecuteAsync(RefreshSetupAsync);
            Shown+=async(_,_)=>await ExecuteAsync(RefreshSetupAsync);
        }
    #endregion Initialization
    #region Account Operations
        private async Task RefreshSetupAsync()
        {
            _activation.Visible=false;_staff.Visible=false;RetryConnection.Visible=false;
            ConnectionStatus.Text="Comprobando servidor…";ConnectionStatus.ForeColor=Obsidian.Muted;
            try
            {
                var required=await _session.User.IsSetupRequiredAsync();_activation.Visible=required;_staff.Visible=!required;
                ConnectionStatus.ForeColor=Obsidian.Mint;ConnectionStatus.Text="●  Servidor conectado";
            }
            catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                ConnectionStatus.ForeColor=Color.FromArgb(255,174,165);ConnectionStatus.Text=error is InvalidOperationException?"●  El servidor respondió; no se pudo comprobar la activación":"●  Conexión no disponible";RetryConnection.Visible=true;throw;
            }
        }
        private async Task LoginAsync()
        {
            await _session.User.LoginAsync(new LoginData{Email=_email.Input.Text.Trim(),Password=_password.Input.Text});_password.Input.Clear();
            Hide();
            try{using var form=new AdminForm(_session);form.ShowDialog(this);}
            finally{Show();await CloseSessionAsync();}
            await RefreshSetupAsync();
        }
        private async Task RegisterAsync()
        {
            using var editor=new AccountActivationForm(data=>_session.User.RegisterInitialAsync(data));
            if(await ShowPageAsync(editor)==DialogResult.OK)
            {
                _email.Input.Text=editor.Data.Email;
                MessageBox.Show(this,"Cuenta creada. Ya puedes iniciar sesión.","Activación completada",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            await RefreshSetupAsync();
        }
        /// <summary>Uses a temporary owner session for account registration and signs it out when the flow ends.</summary>
        private async Task RegisterStaffAsync()
        {
            try
            {
                using var authorization=new OwnerAuthorizationForm(_session);
                if(await ShowPageAsync(authorization)!=DialogResult.OK)return;
                if(await StaffAccountForm.OpenAsync(this,_session)==DialogResult.OK)MessageBox.Show(this,"La cuenta se registró correctamente.","Cuenta de empleado",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            finally{await CloseSessionAsync();}
            await RefreshSetupAsync();
        }
        private async Task CloseSessionAsync()
        {
            try{await _session.User.LogoutAsync();}catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException){ }
        }
        private async Task ExecuteAsync(Func<Task> action)
        {
            if(_busy)return;_busy=true;using var locked=new FieldBusyState(Body);RetryConnection.Enabled=false;_errors.Text=string.Empty;
            try{await action();}
            catch(Exception error)when(error is HttpRequestException or TaskCanceledException or InvalidOperationException or ValidationException)
            {
                _errors.Text=error is HttpRequestException or TaskCanceledException?"No se pudo completar la conexión. Revisa que la API esté abierta.":error.Message;
                if(error is HttpRequestException or TaskCanceledException){ConnectionStatus.Text="●  Conexión no disponible";ConnectionStatus.ForeColor=Color.FromArgb(255,174,165);RetryConnection.Visible=true;}
                Fields.ScrollControlIntoView(_errors);
            }
            finally{_busy=false;if(!IsDisposed){RetryConnection.Enabled=true;}}
        }
    #endregion Account Operations
}
