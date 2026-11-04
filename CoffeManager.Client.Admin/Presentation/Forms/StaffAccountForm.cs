using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Collects an owner-authorized staff credential and optional atomic employee assignment.</summary>
internal sealed class StaffAccountForm : ObsidianAccessForm
{
    #region Operation Data
        private readonly AdminSession _session;
        private readonly ObsidianField _first,_paternal,_maternal,_email,_password,_confirmation;
        private readonly ComboBox _employee,_branch,_role;
        private readonly Label _errors;
        private bool _saving;
        private bool _refreshing;
        private IReadOnlyList<EmployeeDetailsData> _employees;
        private readonly Label _assignmentNote;
        private readonly CheckBox _link;
        private readonly List<Control> _assignmentControls=[];
        private sealed record Choice(Guid? Id,string Name);
    #endregion Operation Data
    #region Initialization
        internal StaffAccountForm(AdminSession session,IReadOnlyList<EmployeeDetailsData> employees,IReadOnlyList<BranchDetailsData> branches,IReadOnlyList<RoleData> roles):base("Cuenta para empleado",1260,900)
        {
            _session=session;_employees=employees;ConnectionStatus.Text="Propietario autenticado · Registro de cuenta";ConnectionStatus.ForeColor=Obsidian.Mint;
            AddHeading("Una cuenta para tu equipo.","Registra el acceso de un empleado, antes o después de crear su perfil laboral.");
            _first=AddField("Nombre(s)","Nombre del titular de la cuenta",80);
            (_paternal,_maternal)=AddPair("Apellido paterno","Apellido paterno",80,"Apellido materno (opcional)","Apellido materno",80);
            _email=AddField("Correo electrónico","cuenta@correo.com",254);
            (_password,_confirmation)=AddPair("Contraseña","12 a 128 caracteres",128,"Confirmar contraseña","Repite tu contraseña",128,true);
            _link=new ObsidianCheckBox{Text="Vincular ahora a un empleado existente",AutoSize=false,ForeColor=Obsidian.Text,BackColor=Obsidian.Background,Font=new Font("Segoe UI",10),Margin=new Padding(0,6,0,12)};Fields.Controls.Add(_link);
            _employee=AddSelector("Empleado",employees.Where(x=>x.Assignment.UserAccountId is null).Select(x=>new Choice(x.EmployeeId,$"{x.Identity.FirstName} {x.Identity.PaternalLastName} {x.Identity.MaternalLastName}")),"Selecciona un empleado sin cuenta");
            _branch=AddSelector("Sucursal",branches.Select(x=>new Choice(x.BranchId,x.Name)),"Sin sucursal asignada");
            _role=AddSelector("Rol",roles.Select(x=>new Choice(x.RoleId,x.Name)),"Sin rol asignado");
            Fields.Controls.Add(Obsidian.Link("+ Agregar rol",async(_,_)=>await AddRoleAsync()));
            Fields.Controls.Add(Obsidian.Link("Actualizar empleados, sucursales y roles",async(_,_)=>await RefreshOptionsAsync()));
            _assignmentNote=Obsidian.Label(string.Empty,10);_assignmentNote.ForeColor=Obsidian.Muted;Fields.Controls.Add(_assignmentNote);
            var note=Obsidian.Label("Si la cuenta se crea primero, podrás vincularla después desde Empleados. Si el empleado ya existe, selecciónalo con su sucursal y rol. El acceso depende de esa asignación.",10);note.ForeColor=Obsidian.Muted;Fields.Controls.Add(note);
            _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);Fields.Controls.Add(_errors);
            var save=Obsidian.Action("Registrar cuenta",async(_,_)=>await SaveAsync());
            var cancel=Obsidian.Action("Cancelar",(_,_)=>{DialogResult=DialogResult.Cancel;Close();},false);
            AddFormActions(save,cancel);
            void UpdateAssignmentVisibility()
            {
                foreach(var control in _assignmentControls)if(control is ObsidianSelector selector)selector.Input.Enabled=_link.Checked;
                _assignmentNote.Text=_link.Checked?"Selecciona empleado, sucursal y rol. La API validará la asignación al guardar.":"Cuenta sin vincular: la sucursal y el rol se asignarán después desde Empleados. Puedes consultar o ampliar el catálogo de roles ahora.";
                if(!_link.Checked){_employee.SelectedIndex=0;_branch.SelectedIndex=0;_role.SelectedIndex=0;}
                Fields.PerformLayout();
            }
            _link.CheckedChanged+=(_,_)=>UpdateAssignmentVisibility();
            _link.Checked=employees.Any(x=>x.Assignment.UserAccountId is null);UpdateAssignmentVisibility();
            _employee.SelectedIndexChanged+=(_,_)=>
            {
                var id=Selected(_employee);if(id is null||_refreshing)return;var match=_employees.FirstOrDefault(x=>x.EmployeeId==id);if(match.EmployeeId==Guid.Empty)return;
                _first.Input.Text=match.Identity.FirstName;_paternal.Input.Text=match.Identity.PaternalLastName;_maternal.Input.Text=match.Identity.MaternalLastName??string.Empty;
            };
        }
    #endregion Initialization
    #region Account Operations
        private ComboBox AddSelector(string label,IEnumerable<Choice> values,string empty)
        {
            var caption=Obsidian.Label(label);Fields.Controls.Add(caption);_assignmentControls.Add(caption);
            var surface=new ObsidianSelector(label);var combo=surface.Input;combo.DisplayMember=nameof(Choice.Name);
            Fill(combo,values,empty,null);Fields.Controls.Add(surface);_assignmentControls.Add(surface);return combo;
        }
        private static Guid? Selected(ComboBox combo)=>(combo.SelectedItem as Choice)?.Id;
        /// <summary>Loads the same API-backed options for Home, Employees and the owner-authorized access entry point.</summary>
        internal static async Task<DialogResult> OpenAsync(IWin32Window owner,AdminSession session)
        {
            var employees=await session.Employee.GetEmployeesAsync()??[];
            var branches=await session.Branch.GetAsync();var roles=await session.User.GetRolesAsync();
            using var form=new StaffAccountForm(session,employees,branches,roles);return owner is StudioForm studio?await studio.ShowPageAsync(form):form.ShowDialog(owner);
        }
        private static void Fill(ComboBox combo,IEnumerable<Choice> values,string empty,Guid? selected)
        {
            combo.BeginUpdate();
            try
            {
                combo.Items.Clear();combo.Items.Add(new Choice(null,empty));foreach(var value in values)combo.Items.Add(value);
                combo.SelectedIndex=0;for(int i=1;i<combo.Items.Count;i++)if((combo.Items[i] as Choice)?.Id==selected)combo.SelectedIndex=i;
            }
            finally{combo.EndUpdate();}
        }
        private async Task RefreshOptionsAsync()
        {
            if(_saving)return;_saving=true;using var locked=new FieldBusyState(Body);_errors.Text=string.Empty;
            try
            {
                var employees=await _session.Employee.GetEmployeesAsync()??[];
                var branches=await _session.Branch.GetAsync();var roles=await _session.User.GetRolesAsync();
                var employee=Selected(_employee);var branch=Selected(_branch);var role=Selected(_role);_refreshing=true;
                try
                {
                    _employees=employees;
                    Fill(_employee,employees.Where(x=>x.Assignment.UserAccountId is null).Select(x=>new Choice(x.EmployeeId,$"{x.Identity.FirstName} {x.Identity.PaternalLastName} {x.Identity.MaternalLastName}")),"Selecciona un empleado sin cuenta",employee);
                    Fill(_branch,branches.Select(x=>new Choice(x.BranchId,x.Name)),"Sin sucursal asignada",branch);
                    Fill(_role,roles.Select(x=>new Choice(x.RoleId,x.Name)),"Sin rol asignado",role);
                }
                finally{_refreshing=false;}
                ConnectionStatus.Text="Opciones actualizadas desde el servidor";
            }
            catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException){_errors.Text=error.Message;Fields.ScrollControlIntoView(_errors);}
            finally{_saving=false;}
        }
        private async Task AddRoleAsync()
        {
            if(_saving)return;_saving=true;using var locked=new FieldBusyState(Body);Guid? created=null;
            try
            {
                using var editor=new RoleCreationForm(_session);if(await ShowPageAsync(editor)!=DialogResult.OK)return;created=editor.CreatedId;
                var roles=await _session.User.GetRolesAsync();Fill(_role,roles.Select(x=>new Choice(x.RoleId,x.Name)),"Sin rol asignado",_link.Checked?editor.CreatedId:null);
                ConnectionStatus.Text=_link.Checked?"Rol creado y seleccionado":"Rol creado en el catálogo; se asignará al vincular al empleado";
            }
            catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException or ValidationException){_errors.Text=created is null?error.Message:"El rol se creó, pero no se pudo recargar la lista. Usa Actualizar empleados, sucursales y roles. "+error.Message;Fields.ScrollControlIntoView(_errors);}
            finally{_saving=false;}
        }
        private async Task SaveAsync()
        {
            if(_saving)return;_errors.Text=string.Empty;
            if(_password.Input.Text!=_confirmation.Input.Text){_errors.Text="Las contraseñas no coinciden.";_confirmation.Input.Focus();return;}
            var data=new StaffAccountRegistrationData{FirstName=_first.Input.Text.Trim(),PaternalLastName=_paternal.Input.Text.Trim(),MaternalLastName=string.IsNullOrWhiteSpace(_maternal.Input.Text)?null:_maternal.Input.Text.Trim(),Email=_email.Input.Text.Trim(),Password=_password.Input.Text,EmployeeId=_link.Checked?Selected(_employee):null,BranchId=_link.Checked?Selected(_branch):null,RoleId=_link.Checked?Selected(_role):null};
            var errors=new List<ValidationResult>();object value=data;
            if(!Validator.TryValidateObject(value,new ValidationContext(value),errors,true)){_errors.Text=string.Join(Environment.NewLine,errors.Select(x=>x.ErrorMessage));Fields.ScrollControlIntoView(_errors);return;}
            bool linking=_link.Checked;
            if(linking&&(data.EmployeeId is null||data.BranchId is null||data.RoleId is null)){_errors.Text="Selecciona empleado, sucursal y rol, o deja los tres sin asignar.";Fields.ScrollControlIntoView(_errors);return;}
            _saving=true;using var locked=new FieldBusyState(Body);
            try{await _session.User.RegisterStaffAsync(data);_saving=false;DialogResult=DialogResult.OK;Close();}
            catch(Exception error)when(error is HttpRequestException or InvalidOperationException or TaskCanceledException or ValidationException){_errors.Text=error.Message;Fields.ScrollControlIntoView(_errors);}
            finally{_saving=false;}
        }
        protected override void OnFormClosing(FormClosingEventArgs e){if(_saving){e.Cancel=true;return;}base.OnFormClosing(e);}
    #endregion Account Operations
}
