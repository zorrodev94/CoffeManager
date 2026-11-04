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
        private readonly ObsidianFlowPanel _content;
        private readonly Panel _workspace;
        private UserProfileEditor? _profileEditor;
        private readonly ObsidianScrollPanel _navigation;
        private bool _menuArranging;
        private readonly TableLayoutPanel _layout;
        private string? _activePage;
        private readonly ToolTip _navigationTips=new();
        private readonly List<Image> _pageImages=[];
        private bool _busy;
        private bool _expanded=true;
    #endregion Operation Data
    #region Initialization
        internal AdminForm(AdminSession session)
        {
            _session=session;Text="CoffeManager · Administración";Size=new Size(1280,820);MinimumSize=new Size(640,480);
            StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",11);AutoScaleDimensions=new SizeF(96F,96F);AutoScaleMode=AutoScaleMode.None;BackColor=MarfilCafe.Ivory;
            _layout=new BufferedNavigationLayout{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,235));_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));_layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            _navigation=new ObsidianScrollPanel{DecoratedBackground=false,Dock=DockStyle.Fill,BackColor=Obsidian.Surface,Padding=new Padding(12),Margin=Padding.Empty};
            var menu=new NavigationButton("Menú","menu");menu.SetCompact(true);menu.Click+=(_,_)=>ToggleMenu();_navigation.Controls.Add(menu);_navigationTips.SetToolTip(menu,"Mostrar u ocultar nombres del menú");
            _navigation.Controls.Add(new NavigationButton("CoffeManager","brand"){Enabled=false,TabStop=false});
            AddNavigation("Inicio",ShowHomeAsync);AddNavigation("Mi perfil",ShowProfileAsync);AddNavigation("Negocio",ShowBusinessAsync);AddNavigation("Sucursales",ShowBranchesAsync);AddNavigation("Empleados",ShowEmployeesAsync);
            AddNavigation("Cerrar sesión",()=>{Close();return Task.CompletedTask;});
            _content=new ObsidianFlowPanel{Dock=DockStyle.Fill,Padding=new Padding(24)};
            _workspace=new Panel{Dock=DockStyle.Fill,Margin=Padding.Empty};_workspace.Controls.Add(_content);
            _layout.Controls.Add(_navigation,0,0);_layout.Controls.Add(_workspace,1,0);Body.Controls.Add(_layout);
            _navigation.ArrangeRequested+=ApplyMenuLayout;
            Body.SizeChanged+=(_,_)=>ApplyMenuLayout();
            _content.SizeChanged+=(_,_)=>ResizeContent();_content.ControlAdded+=(_,_)=>ResizeContent();
            Shown+=async(_,_)=>{ApplyChromeScale();ApplyMenuLayout();await RunAsync(ShowHomeAsync);ResizeContent();};
            DpiChanged+=(_,_)=>{ApplyChromeScale();ApplyMenuLayout();ResizeContent();};
            FormClosed+=(_,_)=>{_navigationTips.Dispose();foreach(var image in _pageImages)image.Dispose();};
        }
        protected override void OnLoad(EventArgs e)
        {
            var area=Screen.FromControl(this).WorkingArea;Size=AccessLayoutMetrics.StartupSize(new Size(1280,820),area.Size,DeviceDpi);
            MinimumSize=new Size(Math.Min((int)(640*DeviceDpi/96f),area.Width-24),Math.Min((int)(480*DeviceDpi/96f),area.Height-24));
            Location=new Point(area.Left+(area.Width-Width)/2,area.Top+(area.Height-Height)/2);base.OnLoad(e);
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
            var button=new NavigationButton(name,icon);button.Click+=async(_,_)=>{if(_busy||name==_activePage)return;if(_profileEditor is not null&&!_profileEditor.ConfirmDiscard(this))return;CloseProfileEditor();await RunAsync(action);};_navigationTips.SetToolTip(button,name);_navigation.Controls.Add(button);
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
            if(_menuArranging)return;_menuArranging=true;
            var scale=DeviceDpi/96f;bool expanded=_expanded&&Body.Width>=880*scale;
            bool railChanged=_layout.ColumnStyles[0].Width!=(expanded?240:76)*scale;
            _layout.SuspendLayout();_navigation.SuspendLayout();_workspace.SuspendLayout();
            try
            {
                _layout.ColumnStyles[0].Width=(expanded?240:76)*scale;
                _navigation.Padding=new Padding((int)((expanded?12:10)*scale));
                int y=(int)(12*scale);
                _content.Padding=new Padding((int)(24*scale));
                foreach(Control item in _navigation.ContentControls)
                {
                    if(item is NavigationButton button)
                    {
                        button.SetCompact(!expanded||button.AccessibleName=="Menú");button.Width=Math.Max(1,(int)((expanded?240:76)*scale)-_navigation.Padding.Horizontal-2);
                    }
                    else
                    {
                        item.Visible=expanded;item.MaximumSize=new Size(Math.Max(1,(int)(235*scale)-_navigation.Padding.Horizontal-2),0);
                        if(item is Label)
                        {
                            item.AutoSize=false;item.Width=Math.Max(1,(int)(235*scale)-_navigation.Padding.Horizontal-2);item.Height=(int)(44*scale);
                        }
                        else if(item is PictureBox picture){picture.Width=(int)(92*scale);picture.Height=(int)(92*scale);}
                    }
                    if(item.Visible)
                    {
                        int itemY=item.AccessibleName=="Cerrar sesión"?Math.Max(y,_navigation.ClientSize.Height-(int)(68*scale)):y;
                        item.Location=new Point(_navigation.Padding.Left,itemY+-_navigation.ScrollOffset);
                        y=itemY+item.Height+(int)(8*scale);
                    }
                }
                _navigation.SetExtent(y);
            }
            finally
            {
                _navigation.ResumeLayout(true);_workspace.ResumeLayout(true);_layout.ResumeLayout(true);_menuArranging=false;
                if(railChanged)
                {
                    // Moving native child windows can leave copied pixels from the previous rail width.
                    // Finish the new layout before repainting the entire workspace and its children.
                    _workspace.PerformLayout();_profileEditor?.PerformLayout();_content.PerformLayout();
                    _layout.Refresh();
                }
            }
        }
        /// <summary>
        /// Begin page for the current entity operation.
        /// </summary>
        /// <param name="title">The title used by this operation.</param>
        /// <param name="description">The description used by this operation.</param>
        private void BeginPage(string title,string description)
        {
            _activePage=title.StartsWith("Buen día")?"Inicio":title;
            foreach(Control item in _navigation.ContentControls)if(item is NavigationButton nav)nav.SelectPage(nav.AccessibleName==title||(title.StartsWith("Buen día")&&nav.AccessibleName=="Inicio"));
            _content.ClearContent();foreach(var image in _pageImages)image.Dispose();_pageImages.Clear();
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
            var width=Math.Max(1,_content.ContentWidth-_content.Padding.Horizontal);
            foreach(Control control in _content.Controls)
            {
                if(control is FlowLayoutPanel card){card.MinimumSize=Size.Empty;card.MaximumSize=new Size(width,0);card.MinimumSize=new Size(width,0);card.Width=width;foreach(Control child in card.Controls)if(child is Label label)label.MaximumSize=new Size(Math.Max(100,width-card.Padding.Horizontal),0);}
                else if(control is Label label)label.MaximumSize=new Size(width,0);
                else if(control is PictureBox picture&&picture.Width>120)picture.Width=Math.Min(width,650);
                else if(control is CoverImagePanel)control.Width=width;
                else if(control is HomeDashboard dashboard){dashboard.Width=width;dashboard.ArrangeDashboard();}
                else if(control is UserProfileView profile){profile.Width=width;profile.ArrangeView();}
            }
        }
        /// <summary>Displays a consistent summary surface for a saved record or an empty state.</summary>
        private void AddSummaryCard(string title,string description,string? details=null)
        {
            var card=new FlowLayoutPanel{AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Obsidian.Surface,Padding=new Padding(22),Margin=new Padding(0,12,0,16),MinimumSize=new Size(480,0)};
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
            BeginPage("Buen día, "+(string.IsNullOrWhiteSpace(user?.Profile.DisplayName)?user?.FirstName:user?.Profile.DisplayName),"Tu negocio, en orden.");
            var dashboard=new HomeDashboard(business?.Profile.TradeName??"Configura tu negocio",branches.Count,employees.Count,user?.IsOwner==true);
            dashboard.OpenBusiness+=async(_,_)=>await RunAsync(ShowBusinessAsync);
            dashboard.OpenBranches+=async(_,_)=>await RunAsync(ShowBranchesAsync);
            dashboard.OpenEmployees+=async(_,_)=>await RunAsync(ShowEmployeesAsync);
            dashboard.AddEmployee+=async(_,_)=>await RunAsync(async()=>{using var editor=new RecordEditor<EmployeeCreationData>("Agregar empleado",new(),_session.MediaConnection);if(editor.ShowDialog(this)!=DialogResult.OK)return;await _session.Employee.RegisterEmployeeAsync(editor.Data);await ShowHomeAsync();});
            dashboard.OpenProfile+=async(_,_)=>await RunAsync(ShowProfileAsync);
            dashboard.RegisterAccount+=async(_,_)=>await RunAsync(async()=>{if(await StaffAccountForm.OpenAsync(this,_session)==DialogResult.OK)await ShowHomeAsync();});
            _content.Controls.Add(new HomeWelcomePanel());_content.Controls.Add(dashboard);ResizeContent();
        }
    #endregion Home Operations
    #region Task Profile Operations
        /// <summary>Displays all saved profile fields and edits the complete profile interaction structure.</summary>
        private async Task ShowProfileAsync()
        {
            var user=await _session.User.GetAsync()??throw new InvalidOperationException("La cuenta no está disponible.");BeginPage("Mi perfil",$"{user.FirstName} {user.PaternalLastName} {user.MaternalLastName} · {user.Email}");
            Image? avatar=user.Profile.AvatarId is Guid avatarId?await LoadImageAsync(avatarId):null;
            Image? cover=user.Profile.CoverImageId is Guid coverId?await LoadImageAsync(coverId):null;
            var view=new UserProfileView(user,avatar,cover);view.EditRequested+=async(_,_)=>await RunAsync(()=>ShowProfileEditorAsync());_content.Controls.Add(view);ResizeContent();
        }
        private async Task ShowProfileEditorAsync()
        {
            var user=await _session.User.GetAsync()??throw new InvalidOperationException("La cuenta no está disponible.");
            Image? avatar=user.Profile.AvatarId is Guid a?await LoadImageAsync(a):null;Image? cover=user.Profile.CoverImageId is Guid c?await LoadImageAsync(c):null;
            _profileEditor=new UserProfileEditor(_session,user.Profile,avatar,cover){Dock=DockStyle.Fill};
            _profileEditor.Saved+=async(_,_)=>{CloseProfileEditor();await RunAsync(ShowProfileAsync);};
            _profileEditor.Cancelled+=(_,_)=>CloseProfileEditor();_workspace.Controls.Add(_profileEditor);_profileEditor.BringToFront();_content.Visible=false;
        }
        private void CloseProfileEditor(){if(_profileEditor is null)return;_profileEditor.Dispose();_profileEditor=null;_content.Visible=true;_content.PerformLayout();}
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(_profileEditor is not null&&!_profileEditor.ConfirmDiscard(this)){e.Cancel=true;return;}base.OnFormClosing(e);
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
            if(isOwner)AddAction("Registrar cuenta de acceso",async()=>{if(await StaffAccountForm.OpenAsync(this,_session)!=DialogResult.OK)return;await ShowEmployeesAsync();});
            if(isOwner)AddAction("Agregar rol",async()=>{using var editor=new RoleCreationForm(_session);if(await ShowPageAsync(editor)!=DialogResult.OK)return;await ShowEmployeesAsync();});
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

/// <summary>Buffers the navigation shell while its columns change size.</summary>
internal sealed class BufferedNavigationLayout : TableLayoutPanel
{
    internal BufferedNavigationLayout()
    {
        DoubleBuffered=true;
        SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.ResizeRedraw,true);
    }
}
