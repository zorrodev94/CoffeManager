using CoffeManager.Client.Admin.Presentation.Windows;
using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Forms;
internal sealed class AdminForm : StudioForm
{
    #region Operation Data
        private readonly AdminSession _session;
        private readonly FlowLayoutPanel _content;
        private readonly FlowLayoutPanel _navigation;
        private readonly TableLayoutPanel _layout;
        private readonly ToolTip _navigationTips=new();
        private readonly List<Image> _pageImages=[];
        private bool _busy;
        private bool _expanded=true;
    #endregion Operation Data
    #region Initialization
        internal AdminForm(AdminSession session)
        {
            _session=session;Text="CoffeManager · Administración";Size=new Size(1280,900);MinimumSize=new Size(850,650);
            StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",11);AutoScaleDimensions=new SizeF(96F,96F);AutoScaleMode=AutoScaleMode.Dpi;BackColor=MarfilCafe.Ivory;
            _layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,235));_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));_layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            _navigation=new FlowLayoutPanel{Dock=DockStyle.Fill,BackColor=MarfilCafe.Espresso,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(16)};
            var menu=new NavigationButton("Contraer menú","menu");menu.Click+=(_,_)=>ToggleMenu();_navigation.Controls.Add(menu);_navigationTips.SetToolTip(menu,"Mostrar u ocultar nombres del menú");
            _navigation.Controls.Add(new PictureBox{Image=BrandResources.Logo,SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(110,110)});
            var brand=MarfilCafe.Text("CoffeManager",18,true);brand.ForeColor=Color.White;_navigation.Controls.Add(brand);
            AddNavigation("Inicio",ShowHomeAsync);AddNavigation("Mi perfil",ShowProfileAsync);AddNavigation("Negocio",ShowBusinessAsync);AddNavigation("Sucursales",ShowBranchesAsync);AddNavigation("Empleados",ShowEmployeesAsync);
            AddNavigation("Cerrar sesión",()=>{Close();return Task.CompletedTask;});
            _content=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(24)};
            _layout.Controls.Add(_navigation,0,0);_layout.Controls.Add(_content,1,0);Body.Controls.Add(_layout);
            _content.SizeChanged+=(_,_)=>ResizeContent();
            Shown+=async(_,_)=>await RunAsync(ShowHomeAsync);
            DpiChanged+=(_,_)=>ApplyMenuLayout();
            FormClosed+=(_,_)=>_navigationTips.Dispose();foreach(var image in _pageImages)image.Dispose();
        }
    #endregion Initialization
    #region Navigation Operations
        /// <summary>
        /// Add navigation for the current entity operation.
        /// </summary>
        /// <param name="name">The name used by this operation.</param>
        /// <param name="action">The action used by this operation.</param>
        private void AddNavigation(string name,Func<Task> action)
        {
            var icon=name switch{"Inicio"=>"home","Mi perfil"=>"person","Negocio"=>"business","Sucursales"=>"branches","Empleados"=>"employees",_=>"logout"};
            var button=new NavigationButton(name,icon);button.Click+=async(_,_)=>await RunAsync(action);_navigationTips.SetToolTip(button,name);_navigation.Controls.Add(button);
        }
        /// <summary>
        /// Toggle menu for the current entity operation.
        /// </summary>
        private void ToggleMenu()
        {
            _expanded=!_expanded;ApplyMenuLayout();
        }
        /// <summary>Applies one consistent rail width to every navigation control without animation.</summary>
        private void ApplyMenuLayout()
        {
            var scale=DeviceDpi/96f;_layout.SuspendLayout();_navigation.SuspendLayout();
            try
            {
                _layout.ColumnStyles[0].Width=(_expanded?235:72)*scale;
                _navigation.Padding=new Padding((int)((_expanded?16:12)*scale));
                foreach(Control item in _navigation.Controls)
                {
                    if(item is NavigationButton button)button.SetCompact(!_expanded);
                    else item.Visible=_expanded;
                }
            }
            finally{_navigation.ResumeLayout(true);_layout.ResumeLayout(true);}
        }
        /// <summary>
        /// Begin page for the current entity operation.
        /// </summary>
        /// <param name="title">The title used by this operation.</param>
        /// <param name="description">The description used by this operation.</param>
        private void BeginPage(string title,string description)
        {
            while(_content.Controls.Count>0)_content.Controls[0].Dispose();foreach(var image in _pageImages)image.Dispose();_pageImages.Clear();
            _content.Controls.Add(MarfilCafe.Text(title,26,true));_content.Controls.Add(MarfilCafe.Text(description));
        }
        /// <summary>
        /// Add action for the current entity operation.
        /// </summary>
        /// <param name="name">The name used by this operation.</param>
        /// <param name="action">The asynchronous operation executed by the button.</param>
        private void AddAction(string name,Func<Task> action)=>_content.Controls.Add(MarfilCafe.Action(name,async(_,_)=>await RunAsync(action)));
        /// <summary>
        /// Run for the current entity operation.
        /// </summary>
        /// <param name="action">The action used by this operation.</param>
        private async Task RunAsync(Func<Task> action)
        {
            if(_busy)return;_busy=true;_navigation.Enabled=false;UseWaitCursor=true;
            try{await action();}
            catch(Exception error) when(error is HttpRequestException or TaskCanceledException or InvalidOperationException or ValidationException)
            {if(!IsDisposed)MessageBox.Show(this,error is HttpRequestException?"No se pudo conectar con la API local.":error.Message,"Revisa la operación");}
            finally{_busy=false;if(!IsDisposed){_navigation.Enabled=true;UseWaitCursor=false;}}
        }
        /// <summary>Keeps cards and text inside the available viewport at the current DPI.</summary>
        private void ResizeContent()
        {
            var width=Math.Max(160,_content.ClientSize.Width-_content.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-8);
            foreach(Control control in _content.Controls)
            {
                if(control is FlowLayoutPanel card){card.MinimumSize=Size.Empty;card.MaximumSize=new Size(width,0);card.MinimumSize=new Size(width,0);card.Width=width;foreach(Control child in card.Controls)if(child is Label label)label.MaximumSize=new Size(Math.Max(100,width-card.Padding.Horizontal),0);}
                else if(control is Label label)label.MaximumSize=new Size(width,0);
                else if(control is PictureBox picture&&picture.Width>120)picture.Width=Math.Min(width,650);
                else if(control is CoverImagePanel)control.Width=width;
            }
        }
        /// <summary>Displays a consistent summary surface for a saved record or an empty state.</summary>
        private void AddSummaryCard(string title,string description,string? details=null)
        {
            var card=new FlowLayoutPanel{AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Color.White,Padding=new Padding(22),Margin=new Padding(0,12,0,16),MinimumSize=new Size(480,0)};
            card.Controls.Add(MarfilCafe.Text(title,19,true));card.Controls.Add(MarfilCafe.Text(description));
            if(!string.IsNullOrWhiteSpace(details))card.Controls.Add(MarfilCafe.Text(details));
            _content.Controls.Add(card);ResizeContent();
        }
    #endregion Navigation Operations
    #region Home Operations
        /// <summary>
        /// Displays the saved business, current record counts and administration entry points.
        /// </summary>
        private async Task ShowHomeAsync()
        {
            var user=await _session.User.GetAsync();var business=await _session.Business.GetAsync();var branches=await _session.Branch.GetAsync();var employees=await _session.Employee.GetEmployeesAsync()??[];
            BeginPage("Bienvenido, "+(user?.Profile.DisplayName??""),"Tu espacio para organizar el negocio y su equipo.");
            _content.Controls.Add(new CoverImagePanel{Source=BrandResources.Background,Width=800,Height=210,AccessibleName="Café y repostería"});
            AddSummaryCard(business?.Profile.TradeName??"Configura tu negocio",$"{branches.Count} sucursales registradas · {employees.Count} empleados registrados");
            AddAction("Completar mi perfil",ShowProfileAsync);AddAction("Abrir negocio",ShowBusinessAsync);AddAction("Gestionar empleados",ShowEmployeesAsync);AddAction("Gestionar sucursales",ShowBranchesAsync);
        }
    #endregion Home Operations
    #region Task Profile Operations
        /// <summary>Displays all saved profile fields and edits the complete profile interaction structure.</summary>
        private async Task ShowProfileAsync()
        {
            var user=await _session.User.GetAsync()??throw new InvalidOperationException("La cuenta no está disponible.");BeginPage("Mi perfil",$"{user.FirstName} {user.PaternalLastName} {user.MaternalLastName} · {user.Email}");
            await AddImagesAsync(user.Profile.AvatarId,user.Profile.CoverImageId);AddSummaryCard(user.Profile.DisplayName,user.Profile.Description??"Completa tu presentación.");AddAllDetails(user.Profile);
            AddAction("Completar o editar perfil",async()=>{using var editor=new RecordEditor<UserProfileData>("Mi perfil",user.Profile,_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.User.UpdateAsync(editor.Data);await ShowProfileAsync();});
        }
    #endregion Task Profile Operations
    #region Task Business Operations
        /// <summary>Handles both minimal business creation and the complete business profile.</summary>
        private async Task ShowBusinessAsync()
        {
            var result=await _session.Business.GetAsync();BeginPage("Negocio","Identidad, presentación y contacto del negocio.");
            if(result is not BusinessDetailsData business)
            {
                AddSummaryCard("Tu negocio comienza aquí","Registra su nombre. Después podrás completar su información.");AddAction("Crear negocio",async()=>
                {
                    using var editor=new RecordEditor<BusinessCreationData>("Crear negocio",new(){BusinessType="Cafeteria"},_session.MediaConnection,new Dictionary<string,IReadOnlyList<EditorOption>>{{"BusinessType",[new("Cafeteria","Cafetería y repostería")]}});
                    if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Business.RegisterAsync(editor.Data);await ShowBusinessAsync();
                });return;
            }
            await AddImagesAsync(business.Profile.LogoId,business.Profile.CoverImageId);AddSummaryCard(business.Profile.TradeName,"Cafetería y repostería");AddAllDetails(business.Profile);
            AddAction("Completar o editar negocio",async()=>{using var editor=new RecordEditor<BusinessProfileData>("Perfil del negocio",business.Profile,_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Business.UpdateAsync(editor.Data);await ShowBusinessAsync();});
        }
    #endregion Task Business Operations
    #region Task Branch Operations
        /// <summary>Lists branches and edits complete branch profiles without changing their business.</summary>
        private async Task ShowBranchesAsync()
        {
            var branches=await _session.Branch.GetAsync();BeginPage("Sucursales","Información y contacto de cada establecimiento.");
            AddAction("Agregar sucursal",async()=>
            {
                var business=await _session.Business.GetAsync()??throw new InvalidOperationException("Configura primero tu negocio.");
                using var editor=new RecordEditor<BranchCreationData>("Agregar sucursal",new(){BusinessId=business.BusinessId},_session.MediaConnection,new Dictionary<string,IReadOnlyList<EditorOption>>{{"BusinessId",[new(business.BusinessId,business.Profile.TradeName)]}});
                if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Branch.RegisterAsync(editor.Data);await ShowBranchesAsync();
            });
            if(branches.Count==0)AddSummaryCard("Todavía no hay sucursales","Agrega el primer establecimiento cuando hayas configurado el negocio.");
            foreach(var branch in branches)
            {
                var selected=branch;AddSummaryCard(selected.Name,selected.Profile.Description??"Perfil por completar");AddAllDetails(selected.Profile);
                AddAction("Completar o editar sucursal",async()=>{using var editor=new RecordEditor<BranchProfileData>("Perfil de sucursal",selected.Profile,_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Branch.UpdateAsync(selected.BranchId,editor.Data);await ShowBranchesAsync();});
            }
        }
    #endregion Task Branch Operations
    #region Task Employee Operations
        /// <summary>Separates minimal employee creation, complete profiles and explicit access assignments.</summary>
        private async Task ShowEmployeesAsync()
        {
            var isOwner=(await _session.User.GetAsync())?.IsOwner==true;var employees=await _session.Employee.GetEmployeesAsync()??[];BeginPage("Empleados","Identidad, información laboral y asignación operativa de tu equipo.");
            AddAction("Agregar empleado",async()=>{using var editor=new RecordEditor<EmployeeCreationData>("Agregar empleado",new(),_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Employee.RegisterEmployeeAsync(editor.Data);await ShowEmployeesAsync();});
            if(isOwner)AddAction("Registrar cuenta de acceso",async()=>{using var editor=new RecordEditor<StaffAccountRegistrationData>("Registrar cuenta de acceso",new(),_session.MediaConnection);editor.AddNote("La cuenta obtiene acceso después de vincularla a un empleado, una sucursal y un rol con permiso.");if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.User.RegisterStaffAsync(editor.Data);await ShowEmployeesAsync();});
            if(isOwner)AddAction("Agregar rol",async()=>{using var editor=new RecordEditor<RoleCreationData>("Agregar rol",new(),_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.User.AddRoleAsync(editor.Data);await ShowEmployeesAsync();});
            if(isOwner)AddAction("Agregar periodicidad de salario",()=>AddCatalogAsync("salary-bases","Periodicidad de salario"));if(isOwner)AddAction("Agregar estado laboral",()=>AddCatalogAsync("employment-statuses","Estado laboral"));
            if(employees.Count==0){AddSummaryCard("Tu equipo comienza aquí","Los empleados que no usan el sistema pueden registrarse sin una cuenta.");return;}
            var roles=await _session.User.GetRolesAsync();var branches=await _session.Branch.GetAsync();
            foreach(var employee in employees)
            {
                var selected=employee;var role=roles.FirstOrDefault(x=>x.RoleId==selected.Assignment.RoleId);var branch=branches.FirstOrDefault(x=>x.BranchId==selected.Assignment.BranchId);
                AddSummaryCard($"{selected.Identity.FirstName} {selected.Identity.PaternalLastName} {selected.Identity.MaternalLastName}",$"{role.Name??"Sin rol asignado"} · {branch.Name??"Sin sucursal asignada"}");
                AddAction("Ver y completar datos",async()=>
                {
                    var statuses=await _session.Employee.GetCatalogAsync("employment-statuses");var salaries=await _session.Employee.GetCatalogAsync("salary-bases");
                    using var editor=new RecordEditor<EmployeeProfileData>("Perfil del empleado",selected.Profile,_session.MediaConnection,new Dictionary<string,IReadOnlyList<EditorOption>>{{"EmploymentStatusId",Options(statuses)},{"SalaryBasisId",Options(salaries)}});
                    if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Employee.UpdateEmployeeAsync(selected.EmployeeId,editor.Data);await ShowEmployeesAsync();
                });
                if(isOwner)AddAction("Asignar sucursal, rol y cuenta",async()=>
                {
                    var accounts=await _session.User.GetAvailableAccountsAsync();var accountOptions=new List<EditorOption>{new(null,"Sin cuenta de acceso")};accountOptions.AddRange(accounts.Select(x=>new EditorOption(x.UserAccountId,x.DisplayName+" · "+x.Email)));
                    if(selected.Assignment.UserAccountId is Guid current&&!accountOptions.Any(x=>Equals(x.Value,current)))accountOptions.Add(new(current,"Cuenta actual (conservar)"));
                    var roleOptions=new List<EditorOption>{new(null,"Sin rol asignado")};roleOptions.AddRange(roles.Select(x=>new EditorOption(x.RoleId,x.Name+(x.CanAccessAdministration?" · Administración":x.CanAccessCheckout?" · Caja":" · Sin acceso"))));
                    var branchOptions=new List<EditorOption>{new(null,"Sin sucursal asignada")};branchOptions.AddRange(branches.Select(x=>new EditorOption(x.BranchId,x.Name)));
                    using var editor=new RecordEditor<EmployeeAssignmentData>("Asignación del empleado",selected.Assignment,_session.MediaConnection,new Dictionary<string,IReadOnlyList<EditorOption>>{{"RoleId",roleOptions},{"BranchId",branchOptions},{"UserAccountId",accountOptions}});
                    editor.AddNote("Los roles con acceso requieren cuenta y sucursal. Un cambio de rol conserva la contraseña y termina las sesiones anteriores; un rol sin permisos no podrá entrar a la aplicación.");
                    if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Employee.AssignEmployeeAsync(selected.EmployeeId,editor.Data);await ShowEmployeesAsync();
                });
            }
        }
        private static IReadOnlyList<EditorOption> Options(IReadOnlyList<CatalogData> values)=>new[]{new EditorOption(null,"Sin especificar")}.Concat(values.Select(x=>new EditorOption(x.Id,x.Name))).ToList();
        private async Task AddCatalogAsync(string catalog,string title)
        {
            using var editor=new RecordEditor<CatalogCreationData>(title,new(),_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Employee.AddCatalogOptionAsync(catalog,editor.Data);await ShowEmployeesAsync();
        }
    #endregion Task Employee Operations
    #region Profile Presentation
        /// <summary>Shows every persisted profile field using the same Spanish labels as its editor.</summary>
        private void AddAllDetails<T>(T data) where T:struct
        {
            var values=new List<string>();foreach(var property in typeof(T).GetProperties())
            {
                if(property.Name is "AvatarId" or "CoverImageId" or "LogoId")continue;var value=property.GetValue(data);
                string text=value switch{null=>"Sin registrar",DateOnly date=>date.ToString("dd/MM/yyyy"),decimal amount=>amount.ToString("C2",System.Globalization.CultureInfo.GetCultureInfo("es-MX")),Guid=>"Configurado",_=>string.IsNullOrWhiteSpace(value.ToString())?"Sin registrar":value.ToString()!};
                values.Add(RecordEditor<T>.Label(property.Name)+" · "+text);
            }
            AddSummaryCard("Información del perfil",string.Join(Environment.NewLine,values));
        }
        private async Task AddImagesAsync(Guid? avatar,Guid? cover)
        {
            if(cover is Guid coverId){var image=await LoadImageAsync(coverId);if(image is not null)_content.Controls.Add(new CoverImagePanel{Source=image,Width=760,Height=180,AccessibleName="Portada del perfil"});}
            if(avatar is Guid avatarId){var image=await LoadImageAsync(avatarId);if(image is not null){_content.Controls.Add(new PictureBox{Image=image,SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(90,90),AccessibleName="Imagen del perfil"});return;}}
            _content.Controls.Add(new DefaultAvatar());
        }
        private async Task<Image?> LoadImageAsync(Guid id)
        {
            try{var bytes=await _session.MediaConnection.GetImageAsync(id);using var stream=new MemoryStream(bytes);using var source=Image.FromStream(stream);var image=new Bitmap(source);_pageImages.Add(image);return image;}
            catch(Exception error) when(error is HttpRequestException or InvalidOperationException or ArgumentException){return null;}
        }
    #endregion Profile Presentation
}
