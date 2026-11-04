using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Globalization;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Theme;
namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Edits the complete personal profile inside the administration workspace and retains fields after API failures.</summary>
internal sealed class UserProfileEditor : UserControl
{
    private readonly AdminSession _session;
    private readonly AccessStackPanel _fields;
    private readonly ProfileEditorBody _scroll;
    private readonly ProfileMediaPanel _media;
    private readonly Panel _footer;
    private readonly Label _title,_errors;
    private readonly Button _save,_cancel;
    private readonly ObsidianField _display,_description,_phone,_alternate,_birth,_address,_city,_state,_postal,_rfc,_curp;
    private readonly List<Image> _ownedImages=[];
    private Guid? _avatarId,_coverId;
    private bool _busy,_dirty;
    internal event EventHandler? Saved,Cancelled;
    internal bool IsBusy=>_busy;
    internal UserProfileEditor(AdminSession session,UserProfileData profile,Image? avatar,Image? cover)
    {
        _session=session;_avatarId=profile.AvatarId;_coverId=profile.CoverImageId;BackColor=Obsidian.Background;AutoScaleMode=AutoScaleMode.None;
        _title=Obsidian.Label("Editar perfil",24,true);_title.AutoSize=false;
        _fields=new AccessStackPanel{MaximumLogicalWidth=900,CenterVertically=false};
        ObsidianField Field(string label,string value,int max,bool multiline=false)
        {
            _fields.Controls.Add(Obsidian.Label(label));var field=new ObsidianField(label,string.Empty,max,false,multiline);field.Input.Text=value;
            if(multiline)field.Tag="multiline";_fields.Controls.Add(field);return field;
        }
        (ObsidianField,ObsidianField) Pair(string first,string firstValue,int firstMax,string second,string secondValue,int secondMax)
        {
            var pair=new AccessPairPanel(first,string.Empty,firstMax,second,string.Empty,secondMax,false);pair.First.Input.Text=firstValue;pair.Second.Input.Text=secondValue;_fields.Controls.Add(pair);return(pair.First,pair.Second);
        }
        _display=Field("Nombre para mostrar *",profile.DisplayName,100);
        _description=Field("Descripción",profile.Description??"",1000,true);
        (_birth,_phone)=Pair("Fecha de nacimiento (dd/MM/aaaa, opcional)",profile.DateOfBirth?.ToString("dd/MM/yyyy")??"",10,"Teléfono",profile.PhoneNumber??"",30);
        _alternate=Field("Teléfono alternativo",profile.AlternatePhoneNumber??"",30);
        _address=Field("Calle, número y domicilio",profile.Address??"",250);
        (_city,_state)=Pair("Ciudad",profile.City??"",100,"Estado",profile.State??"",100);
        (_postal,_rfc)=Pair("Código postal",profile.PostalCode??"",10,"RFC",profile.TaxIdentificationNumber??"",13);
        _curp=Field("CURP",profile.NationalIdentificationNumber??"",18);
        _errors=Obsidian.Label(string.Empty,10);_errors.ForeColor=Color.FromArgb(255,174,165);_fields.Controls.Add(_errors);
        var note=Obsidian.Label("Los datos opcionales pueden completarse después. Deja un campo vacío para quitar su valor anterior.",9);note.ForeColor=Obsidian.Muted;_fields.Controls.Add(note);
        _media=new ProfileMediaPanel(avatar,cover,profile.DisplayName);_media.ChooseAvatar+=async(_,_)=>await ChooseImageAsync(false);_media.ChooseCover+=async(_,_)=>await ChooseImageAsync(true);
        _media.RemoveAvatar+=(_,_)=>{if(_busy)return;_avatarId=null;_media.SetAvatar(null);_dirty=true;};_media.RemoveCover+=(_,_)=>{if(_busy)return;_coverId=null;_media.SetCover(null);_dirty=true;};
        _scroll=new ProfileEditorBody(_fields,_media){Dock=DockStyle.Fill};
        _footer=new Panel{Dock=DockStyle.Bottom,BackColor=Obsidian.Surface};
        _save=Obsidian.Action("Guardar cambios",async(_,_)=>await SaveAsync());_cancel=Obsidian.Action("Cancelar",(_,_)=>{if(ConfirmDiscard(this))Cancelled?.Invoke(this,EventArgs.Empty);},false);_footer.Controls.AddRange([_cancel,_save]);
        Controls.Add(_scroll);Controls.Add(_footer);Controls.Add(_title);
        foreach(var field in new[]{_display,_description,_birth,_phone,_alternate,_address,_city,_state,_postal,_rfc,_curp})field.Input.TextChanged+=(_,_)=>_dirty=true;
        Resize+=(_,_)=>ArrangeEditor();DpiChangedAfterParent+=(_,_)=>ArrangeEditor();ArrangeEditor();
    }
    private void ArrangeEditor()
    {
        if(_footer is null)return;int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);bool narrow=Width<P(420);
        _title.Dock=DockStyle.Top;_title.Height=P(76);_title.Padding=new Padding(P(24),P(18),0,0);_footer.Height=P(narrow?116:76);
        int width=Math.Max(1,Width-P(40));if(narrow){_save.SetBounds(P(20),P(10),width,P(42));_cancel.SetBounds(P(20),P(62),width,P(42));}
        else{_save.SetBounds(Math.Max(P(20),Width-P(214)),P(16),P(190),P(44));_cancel.SetBounds(Math.Max(P(20),Width-P(360)),P(16),P(130),P(44));}
        _scroll.PerformLayout();
    }
    internal bool ConfirmDiscard(IWin32Window owner)
    {
        if(_busy){_errors.Text="Espera a que termine la operación antes de salir.";_scroll.ScrollControlIntoView(_errors);return false;}
        return !_dirty||MessageBox.Show(owner,"¿Descartar los cambios del perfil que todavía no guardaste?","Cambios sin guardar",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
    }
    private UserProfileData Build()
    {
        string? Optional(ObsidianField field)=>string.IsNullOrWhiteSpace(field.Input.Text)?null:field.Input.Text.Trim();
        DateOnly? birth=null;var text=Optional(_birth);
        if(text is not null){if(!DateOnly.TryParseExact(text,"dd/MM/yyyy",CultureInfo.GetCultureInfo("es-MX"),DateTimeStyles.None,out var value))throw new ValidationException("Escribe la fecha como dd/MM/aaaa o déjala vacía.");if(value>DateOnly.FromDateTime(DateTime.Today))throw new ValidationException("La fecha de nacimiento no puede ser futura.");birth=value;}
        var data=new UserProfileData{DisplayName=_display.Input.Text.Trim(),Description=Optional(_description),PhoneNumber=Optional(_phone),AlternatePhoneNumber=Optional(_alternate),DateOfBirth=birth,Address=Optional(_address),City=Optional(_city),State=Optional(_state),PostalCode=Optional(_postal),TaxIdentificationNumber=Optional(_rfc),NationalIdentificationNumber=Optional(_curp),AvatarId=_avatarId,CoverImageId=_coverId};
        object boxed=data;Validator.ValidateObject(boxed,new ValidationContext(boxed),true);return data;
    }
    private async Task SaveAsync()
    {
        if(_busy)return;_errors.Text=string.Empty;
        try
        {
            var data=Build();_busy=true;using var locked=new FieldBusyState(this);
            await _session.User.UpdateAsync(data);_dirty=false;_busy=false;Saved?.Invoke(this,EventArgs.Empty);
        }
        catch(Exception error)when(error is ValidationException or HttpRequestException or InvalidOperationException or TaskCanceledException){_errors.Text=error.Message;_scroll.ScrollControlIntoView(_errors);}
        finally{_busy=false;}
    }
    private async Task ChooseImageAsync(bool cover)
    {
        if(_busy)return;using var dialog=new OpenFileDialog{Title=cover?"Elegir portada":"Elegir foto de perfil",Filter="PNG o JPEG|*.png;*.jpg;*.jpeg",CheckFileExists=true};if(dialog.ShowDialog(this)!=DialogResult.OK)return;
        _busy=true;using var locked=new FieldBusyState(this);_errors.Text=string.Empty;
        try
        {
            var bytes=PrepareImage(dialog.FileName);var id=await _session.MediaConnection.UploadImageAsync(bytes);
            using var stream=new MemoryStream(bytes);using var source=Image.FromStream(stream);var image=new Bitmap(source);_ownedImages.Add(image);
            if(cover){_coverId=id;_media.SetCover(image);}else{_avatarId=id;_media.SetAvatar(image);}_dirty=true;
        }
        catch(Exception error)when(error is IOException or ArgumentException or ValidationException or HttpRequestException or InvalidOperationException or TaskCanceledException){_errors.Text=error.Message;_scroll.ScrollControlIntoView(_errors);}
        finally{_busy=false;}
    }
    /// <summary>Applies the same JPEG dimensions and byte limits used by the existing media workflow.</summary>
    internal static byte[] PrepareImage(string path)
    {
        if(new FileInfo(path).Length>20*1024*1024)throw new ValidationException("La imagen de origen supera 20 MB.");
        using var source=Image.FromFile(path);if((long)source.Width*source.Height>20000000)throw new ValidationException("La imagen de origen supera 20 megapíxeles.");
        double scale=Math.Min(1,1600d/Math.Max(source.Width,source.Height));using var bitmap=new Bitmap(Math.Max(1,(int)(source.Width*scale)),Math.Max(1,(int)(source.Height*scale)));
        using(var graphics=Graphics.FromImage(bitmap)){graphics.Clear(Color.White);graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;graphics.DrawImage(source,0,0,bitmap.Width,bitmap.Height);}
        using var output=new MemoryStream();using var settings=new EncoderParameters(1);settings.Param[0]=new EncoderParameter(System.Drawing.Imaging.Encoder.Quality,85L);bitmap.Save(output,ImageCodecInfo.GetImageEncoders().Single(x=>x.MimeType=="image/jpeg"),settings);
        if(output.Length>2*1024*1024)throw new ValidationException("La imagen preparada supera 2 MB.");return output.ToArray();
    }
    protected override void Dispose(bool disposing){if(disposing)foreach(var image in _ownedImages)image.Dispose();base.Dispose(disposing);}
}
internal sealed class ProfileEditorBody : ObsidianScrollPanel
{
    private readonly AccessStackPanel _fields;
    private readonly ProfileMediaPanel _media;
    private bool _arranging;
    internal ProfileEditorBody(AccessStackPanel fields,ProfileMediaPanel media)
    {
        _fields=fields;_media=media;Controls.AddRange([fields,media]);
        _fields.Layout+=(_,_)=>{if(!_arranging&&_fields.Height!=_fields.ContentHeight)PerformLayout();};
    }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_arranging||_fields is null)return;_arranging=true;
        try
        {
            int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);int inset=P(16),gap=P(12),width=Math.Max(1,ContentWidth-inset*2);bool columns=width>=P(880);int mediaWidth=columns?P(220):width;
            int fieldWidth=columns?Math.Max(1,width-mediaWidth-gap):width;
            // Scrolling only moves existing content; remeasure fields when their width changes.
            if(_fields.Width!=fieldWidth||_fields.Height<1)
            {
                _fields.SetBounds(columns?inset+mediaWidth+gap:inset,0,fieldWidth,P(2400));
                _fields.PerformLayout();
            }
            int fieldHeight=_fields.ContentHeight;
            _fields.SetBounds(columns?inset+mediaWidth+gap:inset,_fields.Top,fieldWidth,fieldHeight);
            int mediaHeight=_media.RequiredHeight(mediaWidth);_media.SetBounds(inset,inset-ScrollOffset,mediaWidth,mediaHeight);
            _fields.Top=(columns?inset:inset+mediaHeight+gap)-ScrollOffset;
            SetExtent((columns?Math.Max(mediaHeight,fieldHeight):mediaHeight+gap+fieldHeight)+inset*2);
        }
        finally{_arranging=false;}
    }
}
internal sealed class ProfileMediaPanel : Panel
{
    private readonly CircularAvatar _avatar;
    private readonly CoverImagePanel _cover;
    private readonly Button _photo,_removePhoto,_coverButton,_removeCover;
    private readonly Label _avatarHint,_coverHint;
    internal event EventHandler? ChooseAvatar,ChooseCover,RemoveAvatar,RemoveCover;
    internal ProfileMediaPanel(Image? avatar,Image? cover,string displayName)
    {
        BackColor=Obsidian.Surface;_avatar=new CircularAvatar{Source=avatar,Initials=string.Concat(displayName.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x=>x[0])).ToUpperInvariant(),AccessibleName="Vista previa del avatar"};_cover=new CoverImagePanel{Fit=true,Source=cover,AccessibleName="Vista previa de portada",BackColor=Obsidian.Background};
        _photo=Obsidian.Action("Cambiar foto",(_,e)=>ChooseAvatar?.Invoke(this,e),false);_removePhoto=Obsidian.Action("Quitar foto",(_,e)=>RemoveAvatar?.Invoke(this,e),false);
        _coverButton=Obsidian.Action("Cambiar portada",(_,e)=>ChooseCover?.Invoke(this,e),false);_removeCover=Obsidian.Action("Quitar portada",(_,e)=>RemoveCover?.Invoke(this,e),false);_avatarHint=Obsidian.Label("512 × 512 px · JPG/PNG",9);_coverHint=Obsidian.Label("1600 × 400 px · JPG/PNG",9);
        _avatarHint.AutoSize=false;_coverHint.AutoSize=false;_avatarHint.ForeColor=Obsidian.Muted;_coverHint.ForeColor=Obsidian.Muted;
        Controls.AddRange([_avatar,_cover,_photo,_removePhoto,_coverButton,_removeCover,_avatarHint,_coverHint]);
    }
    internal void SetAvatar(Image? image){_avatar.Source=image;_avatar.Invalidate();}
    internal void SetCover(Image? image){_cover.Source=image;_cover.Invalidate();}
    internal int RequiredHeight(int width)=>(int)((width>=450*DeviceDpi/96f?284:560)*DeviceDpi/96f);
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);if(_avatar is null)return;int P(int n)=>(int)Math.Round(n*DeviceDpi/96f);bool side=Width>=P(450);int w=side?(Width-P(48))/2:Math.Max(1,Width-P(32)),x=P(16),coverX=side?x+w+P(16):x,coverY=side?P(16):P(292);
        _avatar.SetBounds(x+Math.Max(0,(w-P(112))/2),P(16),Math.Min(w,P(112)),P(112));_avatarHint.SetBounds(x,P(136),w,P(42));_photo.SetBounds(x,P(188),w,P(38));_removePhoto.SetBounds(x,P(234),w,P(34));
        _cover.SetBounds(coverX,coverY,w,P(112));_coverHint.SetBounds(coverX,coverY+P(120),w,P(42));_coverButton.SetBounds(coverX,coverY+P(172),w,P(38));_removeCover.SetBounds(coverX,coverY+P(218),w,P(34));
    }
}
