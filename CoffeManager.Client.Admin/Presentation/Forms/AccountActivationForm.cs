using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Collects and validates only the initial owner activation fields before requesting persistence.</summary>
internal sealed class AccountActivationForm : ObsidianAccessForm
{
    #region Operation Data
        private readonly ObsidianField _first,_paternal,_maternal,_email,_password,_confirmation,_key;
        private readonly Label _errors;
        private readonly Func<AccountRegistrationData,Task<Guid>> _register;
        private bool _saving;
        internal AccountRegistrationData Data { get; private set; }
    #endregion Operation Data
    #region Initialization
        internal AccountActivationForm(Func<AccountRegistrationData,Task<Guid>> register):base("Primera activación",1260,880)
        {
            _register=register;ConnectionStatus.Text="Configuración inicial · Propietario";
            AddHeading("Activa tu primera cuenta","Comienza a administrar tu negocio.");
            _first=AddField("Nombre(s)","Ingresa tu nombre",80);
            (_paternal,_maternal)=AddPair("Apellido paterno","Apellido paterno",80,"Apellido materno (opcional)","Apellido materno",80);
            _email=AddField("Correo electrónico","tu@correo.com",254);
            (_password,_confirmation)=AddPair("Contraseña","12 a 128 caracteres",128,"Confirmar contraseña","Repite tu contraseña",128,true);
            _key=AddField("Clave de activación","Clave entregada por tu proveedor",200,true);
            var role=Obsidian.Label("Propietario · Asignado automáticamente en la primera activación",10);role.ForeColor=Obsidian.Muted;Fields.Controls.Add(role);
            _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);Fields.Controls.Add(_errors);
            var save=Obsidian.Action("Activar cuenta",async(_,_)=>await ConfirmAsync());Fields.Controls.Add(save);AcceptButton=save;
            var cancel=Obsidian.Action("Volver al acceso",(_,_)=>{DialogResult=DialogResult.Cancel;Close();},false);Fields.Controls.Add(cancel);CancelButton=cancel;
            Shown+=(_,_)=>_first.Input.Focus();
        }
    #endregion Initialization
    #region Activation Operations
        private async Task ConfirmAsync()
        {
            if(_saving)return;
            _errors.Text=string.Empty;
            if(_password.Input.Text!=_confirmation.Input.Text){_errors.Text="Las contraseñas no coinciden.";_confirmation.Input.Focus();return;}
            var data=new AccountRegistrationData{FirstName=_first.Input.Text.Trim(),PaternalLastName=_paternal.Input.Text.Trim(),MaternalLastName=string.IsNullOrWhiteSpace(_maternal.Input.Text)?null:_maternal.Input.Text.Trim(),Email=_email.Input.Text.Trim(),Password=_password.Input.Text,SetupKey=_key.Input.Text};
            var errors=new List<ValidationResult>();object value=data;
            if(!Validator.TryValidateObject(value,new ValidationContext(value),errors,true)){_errors.Text=string.Join(Environment.NewLine,errors.Select(x=>x.ErrorMessage));Fields.ScrollControlIntoView(_errors);return;}
            _saving=true;Fields.Enabled=false;
            try
            {
                await _register(data);Data=data;_saving=false;DialogResult=DialogResult.OK;Close();
            }
            catch(Exception error)when(error is HttpRequestException or TaskCanceledException or InvalidOperationException or ValidationException)
            {
                _errors.Text=error is HttpRequestException or TaskCanceledException?"No se pudo completar la conexión. Tus datos siguen aquí; puedes intentarlo nuevamente.":error.Message;
            }
            finally{if(!IsDisposed)Fields.ScrollControlIntoView(_errors);_saving=false;if(!IsDisposed)Fields.Enabled=true;}

        }
        /// <summary>Prevents closing the modal while the server is resolving account creation.</summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(_saving){e.Cancel=true;return;}base.OnFormClosing(e);
        }
        /// <summary>Clears references to activation credentials when the modal operation ends.</summary>
        protected override void Dispose(bool disposing)
        {
            if(disposing)Data=default;base.Dispose(disposing);
        }
    #endregion Activation Operations
}
