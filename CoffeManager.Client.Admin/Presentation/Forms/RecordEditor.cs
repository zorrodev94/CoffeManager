using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Associates one interaction structure with a complete, labelled editor without mixing entity persistence into presentation.</summary>
internal sealed class RecordEditor<T> : IDisposable where T:struct
{
    #region Operation Data
        private readonly EditorForm _form;
        private readonly T _original;
        private readonly Dictionary<string,Func<object?>> _values=[];
        private readonly Dictionary<string,TextBox> _passwordConfirmations=[];
        private readonly List<Image> _images=[];
        private readonly ApiConnection _connection;
        private readonly CancellationTokenSource _lifetime=new();
        internal T Data=>Build();
        internal static readonly Dictionary<string,string> Labels=new()
        {
            ["FirstName"]="Nombre(s)",["PaternalLastName"]="Apellido paterno",["MaternalLastName"]="Apellido materno",["Email"]="Correo electrónico",["Password"]="Contraseña",["SetupKey"]="Clave de activación",
            ["DisplayName"]="Nombre para mostrar",["Description"]="Descripción",["AvatarId"]="Foto de perfil",["CoverImageId"]="Portada",["LogoId"]="Logo",["PhoneNumber"]="Teléfono",["AlternatePhoneNumber"]="Teléfono alternativo",["DateOfBirth"]="Fecha de nacimiento",
            ["Address"]="Calle, número y domicilio",["Neighborhood"]="Colonia",["City"]="Ciudad",["State"]="Estado",["PostalCode"]="Código postal",["TaxIdentificationNumber"]="RFC",["NationalIdentificationNumber"]="CURP",
            ["EmployeeNumber"]="Número de empleado",["HireDate"]="Fecha de contratación",["EmploymentStatusId"]="Estado laboral",["SalaryAmount"]="Salario (MXN)",["SalaryBasisId"]="Periodicidad del salario",
            ["TradeName"]="Nombre para mostrar",["LegalName"]="Nombre legal",["BusinessType"]="Giro",["Name"]="Nombre",["BusinessId"]="Negocio",["BranchId"]="Sucursal",["RoleId"]="Rol",["UserAccountId"]="Cuenta de acceso",
            ["CanAccessAdministration"]="Permitir acceso a Administración",["CanAccessCheckout"]="Permitir acceso a Caja"
        };
    #endregion Operation Data
    #region Initialization
        /// <summary>Creates editors for every public property in the selected interaction structure.</summary>
        internal RecordEditor(string title,T data,ApiConnection connection,IReadOnlyDictionary<string,IReadOnlyList<EditorOption>>? options=null)
        {
            _original=data;_connection=connection;_form=new EditorForm(title);
            _form.AddNote("Los campos con * son obligatorios. Puedes completar después los datos opcionales.");
            string current="";
            foreach(var property in typeof(T).GetProperties())
            {
                var section=Section(property.Name);if(section!=current){_form.AddSection(section);current=section;}
                var label=Label(property.Name)+(property.GetCustomAttribute<RequiredAttribute>() is null?"":" *");var value=property.GetValue(data);
                if(property.Name is "AvatarId" or "LogoId" or "CoverImageId"){AddImage(property,label,(Guid?)value);continue;}
                if(options?.TryGetValue(property.Name,out var choices)==true)
                {
                    var combo=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,FlatStyle=FlatStyle.Flat,BackColor=Color.White,ForeColor=MarfilCafe.Espresso,Font=new Font("Segoe UI",11),AccessibleName=label};
                    foreach(var choice in choices)combo.Items.Add(choice);combo.DisplayMember=nameof(EditorOption.Label);combo.SelectedIndex=choices.ToList().FindIndex(x=>Equals(x.Value,value));if(combo.SelectedIndex<0&&combo.Items.Count>0)combo.SelectedIndex=0;
                    _form.AddEditor(label,combo);_values[property.Name]=()=>((EditorOption?)combo.SelectedItem)?.Value;continue;
                }
                var type=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;
                if(type==typeof(DateOnly))
                {
                    var date=new DateTimePicker{Format=DateTimePickerFormat.Custom,CustomFormat="dd / MM / yyyy",ShowCheckBox=true,Checked=value is not null,Value=value is DateOnly day?day.ToDateTime(TimeOnly.MinValue):DateTime.Today,AccessibleName=label};_form.AddEditor(label,date);_values[property.Name]=()=>date.Checked?DateOnly.FromDateTime(date.Value):null;
                }
                else if(type==typeof(decimal))
                {
                    var input=_form.AddField(label,value is decimal amount?amount.ToString("0.00",CultureInfo.CurrentCulture):"");input.MaxLength=24;
                    _values[property.Name]=()=>string.IsNullOrWhiteSpace(input.Text)?null:decimal.TryParse(input.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out var amount)?amount:throw new ValidationException("Escribe un salario válido.");
                }
                else if(type==typeof(bool))
                {
                    var box=new CheckBox{Text=label,Checked=value is true,AutoSize=true,AccessibleName=label};_form.AddEditor("Permiso",box);_values[property.Name]=()=>box.Checked;
                }
                else if(type==typeof(Guid))throw new InvalidOperationException("Faltan las opciones autorizadas del campo "+label+".");
                else
                {
                    var input=_form.AddField(label,value as string??"",property.Name=="Description");input.MaxLength=property.GetCustomAttribute<StringLengthAttribute>()?.MaximumLength??160;
                    if(property.Name is "Password" or "SetupKey")input.UseSystemPasswordChar=true;
                    if(property.Name=="Password")
                    {
                        var confirmation=_form.AddField("Confirmar contraseña *");confirmation.MaxLength=128;confirmation.UseSystemPasswordChar=true;_passwordConfirmations[property.Name]=confirmation;
                    }
                    _values[property.Name]=()=>string.IsNullOrWhiteSpace(input.Text)?(property.GetCustomAttribute<RequiredAttribute>() is null?null:""):property.Name is "Password" or "SetupKey"?input.Text:input.Text.Trim();
                }
            }
            _form.ValidateBeforeConfirm(()=>Build());_form.FormClosed+=(_,_)=>_lifetime.Cancel();
        }
    #endregion Initialization
    #region Form Operations
        internal DialogResult ShowDialog(IWin32Window owner)=>_form.ShowDialog(owner);
        internal void AddNote(string text)=>_form.AddNote(text);
        private T Build()
        {
            object result=_original;foreach(var property in typeof(T).GetProperties())if(_values.TryGetValue(property.Name,out var getter))property.SetValue(result,getter());
            foreach(var pair in _passwordConfirmations)if((string?)typeof(T).GetProperty(pair.Key)?.GetValue(result)!=pair.Value.Text)throw new ValidationException("Las contraseñas no coinciden.");
            if(typeof(T).GetProperty("DateOfBirth")?.GetValue(result) is DateOnly birth&&birth>DateOnly.FromDateTime(DateTime.Today))throw new ValidationException("La fecha de nacimiento no puede ser futura.");
            if(typeof(T).GetProperty("SalaryAmount")?.GetValue(result) is decimal&&typeof(T).GetProperty("SalaryBasisId")?.GetValue(result) is null)throw new ValidationException("Selecciona la periodicidad del salario.");return (T)result;
        }
        internal static string Label(string name)=>Labels.GetValueOrDefault(name,name);
        internal static string Section(string name)=>name switch
        {
            "AvatarId" or "LogoId" or "CoverImageId"=>"Imagen del perfil",
            "Address" or "Neighborhood" or "City" or "State" or "PostalCode"=>"Ubicación",
            "TaxIdentificationNumber" or "NationalIdentificationNumber"=>"Identificación",
            "EmployeeNumber" or "HireDate" or "EmploymentStatusId" or "SalaryAmount" or "SalaryBasisId"=>"Información laboral",
            "BranchId" or "RoleId" or "UserAccountId"=>"Asignación operativa",
            _=>"Información general"
        };
        private void AddImage(PropertyInfo property,string label,Guid? value)
        {
            Guid? selected=value;var row=new FlowLayoutPanel{AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false};var preview=new PictureBox{Size=new Size(property.Name=="CoverImageId"?420:110,110),SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.White,AccessibleName=label};row.Controls.Add(preview);
            var status=MarfilCafe.Text(value is null?"Sin imagen personalizada":"Imagen configurada",10);row.Controls.Add(status);
            async Task LoadPreview(Guid id)
            {
                var bytes=await _connection.GetImageAsync(id,_lifetime.Token);if(_form.IsDisposed)return;using var source=new MemoryStream(bytes);using var image=Image.FromStream(source);var copy=new Bitmap(image);_images.Add(copy);preview.Image=copy;
            }
            if(value is Guid existing)_form.Shown+=async(_,_)=>{try{await LoadPreview(existing);}catch(Exception error) when(error is HttpRequestException or InvalidOperationException or OperationCanceledException or ArgumentException){if(!_form.IsDisposed)status.Text="No se pudo cargar la vista previa.";}};
            var choose=MarfilCafe.Action("Elegir imagen",async(_,_)=>
            {
                using var dialog=new OpenFileDialog{Title=label,Filter="Imagen PNG o JPEG|*.png;*.jpg;*.jpeg",CheckFileExists=true};if(dialog.ShowDialog(_form)!=DialogResult.OK)return;row.Enabled=false;_form.BeginPendingOperation();
                try
                {
                    if(new FileInfo(dialog.FileName).Length>20*1024*1024)throw new ValidationException("El archivo de origen no debe superar 20 MB.");
                    using var source=Image.FromFile(dialog.FileName);if((long)source.Width*source.Height>20000000)throw new ValidationException("La imagen de origen no debe superar 20 megapíxeles.");
                    var scale=Math.Min(1d,1600d/Math.Max(source.Width,source.Height));using var bitmap=new Bitmap(Math.Max(1,(int)(source.Width*scale)),Math.Max(1,(int)(source.Height*scale)));using(var graphics=Graphics.FromImage(bitmap)){graphics.Clear(Color.White);graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;graphics.DrawImage(source,0,0,bitmap.Width,bitmap.Height);}
                    using var output=new MemoryStream();var encoder=ImageCodecInfo.GetImageEncoders().Single(x=>x.MimeType=="image/jpeg");using var settings=new EncoderParameters(1);settings.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,85L);bitmap.Save(output,encoder,settings);
                    if(output.Length>2*1024*1024)throw new ValidationException("La imagen preparada supera 2 MB. Elige una imagen más sencilla.");
                    selected=await _connection.UploadImageAsync(output.ToArray(),_lifetime.Token);await LoadPreview(selected.Value);if(!_form.IsDisposed)status.Text="Imagen preparada. Guarda el perfil para aplicarla.";
                }
                catch(Exception error) when(error is HttpRequestException or InvalidOperationException or ValidationException or ArgumentException or OperationCanceledException or IOException){if(!_form.IsDisposed&&!_lifetime.IsCancellationRequested)MessageBox.Show(_form,error.Message,"Revisa la imagen");}
                finally{_form.EndPendingOperation();if(!_form.IsDisposed)row.Enabled=true;}
            });row.Controls.Add(choose);row.Controls.Add(MarfilCafe.Action("Quitar imagen",(_,_)=>{selected=null;preview.Image=null;status.Text="Se usará la imagen predeterminada al guardar.";},false));_form.AddEditor(label,row);_values[property.Name]=()=>selected;
        }
        public void Dispose(){_lifetime.Cancel();_form.Dispose();foreach(var image in _images)image.Dispose();_lifetime.Dispose();}
    #endregion Form Operations
}
internal sealed record EditorOption(object? Value,string Label);
