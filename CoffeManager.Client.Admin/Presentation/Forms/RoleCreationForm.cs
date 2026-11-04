using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Keeps a new role's fields available until the owner-authorized API request succeeds.</summary>
internal sealed class RoleCreationForm : ObsidianAccessForm
{
    private readonly AdminSession _session;
    private readonly ObsidianField _name;
    private readonly CheckBox _administration,_checkout;
    private readonly Label _errors;
    private bool _saving;
    internal Guid CreatedId { get; private set; }
    internal RoleCreationForm(AdminSession session):base("Nuevo rol",620,660,false)
    {
        _session=session;AddHeading("Un rol para tu equipo","Define su nombre y los accesos que necesita.");
        _name=AddField("Nombre o apodo del rol","Por ejemplo: Encargado de turno",100);
        _administration=new ObsidianCheckBox{Text="Permitir acceso a Administración",AutoSize=false,ForeColor=Obsidian.Text};
        _checkout=new ObsidianCheckBox{Text="Permitir acceso a Caja",AutoSize=false,ForeColor=Obsidian.Text};Fields.Controls.AddRange([_administration,_checkout]);
        var note=Obsidian.Label("Un rol sin estos permisos no concede acceso a esos clientes. Crear un rol no crea otro propietario.",10);note.ForeColor=Obsidian.Muted;Fields.Controls.Add(note);
        _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);Fields.Controls.Add(_errors);
        var save=Obsidian.Action("Crear rol",async(_,_)=>await SaveAsync());
        var cancel=Obsidian.Action("Cancelar",(_,_)=>{DialogResult=DialogResult.Cancel;Close();},false);
            AddFormActions(save,cancel);
        ConnectionStatus.Text="Catálogo compartido · Solo propietario";
    }
    private async Task SaveAsync()
    {
        if(_saving)return;_errors.Text=string.Empty;
        var data=new RoleCreationData{Name=_name.Input.Text.Trim(),CanAccessAdministration=_administration.Checked,CanAccessCheckout=_checkout.Checked};
        var errors=new List<ValidationResult>();object value=data;
        if(!Validator.TryValidateObject(value,new ValidationContext(value),errors,true)){_errors.Text=string.Join(Environment.NewLine,errors.Select(x=>x.ErrorMessage));Fields.ScrollControlIntoView(_errors);return;}
        _saving=true;using var locked=new FieldBusyState(Body);
        try{CreatedId=await _session.User.AddRoleAsync(data);_saving=false;DialogResult=DialogResult.OK;Close();}
        catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException or ValidationException){_errors.Text=error.Message;Fields.ScrollControlIntoView(_errors);}
        finally{_saving=false;}
    }
    protected override void OnFormClosing(FormClosingEventArgs e){if(_saving){e.Cancel=true;return;}base.OnFormClosing(e);}
}
