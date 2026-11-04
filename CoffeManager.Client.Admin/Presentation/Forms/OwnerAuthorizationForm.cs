using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Authenticates the existing owner before exposing staff account management from the access window.</summary>
internal sealed class OwnerAuthorizationForm : ObsidianAccessForm
{
    #region Operation Data
        private readonly AdminSession _session;
        private readonly ObsidianField _email,_password;
        private readonly Label _errors;
        private bool _busy;
    #endregion Operation Data
    #region Initialization
        internal OwnerAuthorizationForm(AdminSession session):base("Autorizar registro",620,660,false)
        {
            _session=session;ConnectionStatus.Text="Registro reservado al propietario";
            AddHeading("Autoriza una nueva cuenta","Introduce los datos de acceso del propietario para continuar.");
            _email=AddField("Correo del propietario","propietario@correo.com",254);
            _password=AddField("Contraseña","Contraseña del propietario",128,true);
            _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);Fields.Controls.Add(_errors);
            var authorize=Obsidian.Action("Continuar al registro",async(_,_)=>await AuthorizeAsync());Fields.Controls.Add(authorize);AcceptButton=authorize;
            var cancel=Obsidian.Action("Volver al acceso",(_,_)=>{DialogResult=DialogResult.Cancel;Close();},false);Fields.Controls.Add(cancel);CancelButton=cancel;
        }
    #endregion Initialization
    #region Authorization Operations
        private async Task AuthorizeAsync()
        {
            if(_busy)return;_busy=true;Fields.Enabled=false;_errors.Text=string.Empty;
            try
            {
                await _session.User.LoginAsync(new LoginData{Email=_email.Input.Text.Trim(),Password=_password.Input.Text});
                var user=await _session.User.GetAsync();
                if(user?.IsOwner!=true)
                {
                    await _session.User.LogoutAsync();throw new InvalidOperationException("Solo el propietario puede crear cuentas de acceso.");
                }
                _busy=false;DialogResult=DialogResult.OK;Close();
            }
            catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException or ValidationException){_errors.Text=error.Message;Fields.ScrollControlIntoView(_errors);}
            finally{_password.Input.Clear();_busy=false;if(!IsDisposed)Fields.Enabled=true;}
        }
        protected override void OnFormClosing(FormClosingEventArgs e){if(_busy){e.Cancel=true;return;}base.OnFormClosing(e);}
    #endregion Authorization Operations
}
