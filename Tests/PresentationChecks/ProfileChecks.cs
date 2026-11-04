using System.Net;
using System.Text.Json;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Forms;
using CoffeManager.Client.Admin.Presentation.Windows;
namespace CoffeManager.PresentationChecks;
internal static class ProfileChecks
{
    internal static void Run()
    {
        Exception? failure=null;using var transport=new ProfileTransport();using var connection=new ApiConnection(new Uri("https://localhost:7066"),transport);
        var session=new AdminSession(connection);var avatar=Guid.NewGuid();
        using var host=new Form{ClientSize=new Size(1080,740)};
        using var editor=new UserProfileEditor(session,new(){DisplayName="Nombre anterior",PhoneNumber="Eliminar",AvatarId=avatar},null,null){Dock=DockStyle.Fill};host.Controls.Add(editor);
        host.Shown+=async(_,_)=>
        {
            try
            {
                var fields=Walk(editor).OfType<ObsidianField>().ToArray();
                void Set(string name,string value)=>fields.Single(x=>x.Input.AccessibleName==name).Input.Text=value;
                Set("Nombre para mostrar *","Nombre actualizado");Set("Descripción","Presentación guardada");Set("Teléfono","");Set("Teléfono alternativo","6671234567");
                Set("Fecha de nacimiento (dd/MM/aaaa, opcional)","15/03/2003");Set("Calle, número y domicilio","Domicilio de prueba");Set("Ciudad","Culiacán");Set("Estado","Sinaloa");Set("Código postal","80110");Set("RFC","TEST030315AB1");Set("CURP","TEST030315HSLSST01");
                var save=Walk(editor).OfType<Button>().Single(x=>x.Text=="Guardar cambios");int saved=0;editor.Saved+=(_,_)=>saved++;
                save.PerformClick();await WaitForSave(editor);Check(transport.Writes==1&&saved==0,"Rejected profile closed editor");Check(fields.Single(x=>x.Input.AccessibleName=="Ciudad").Input.Text=="Culiacán","Rejected update lost values");
                foreach(var size in new[]{new Size(1080,740),new Size(610,480)})
                {
                    host.ClientSize=size;editor.PerformLayout();foreach(var panel in Walk(editor).OfType<ObsidianScrollPanel>())Check(!panel.AutoScroll,"Native scroll track enabled");
                    var action=host.PointToClient(save.PointToScreen(Point.Empty));Check(host.ClientRectangle.Contains(new Rectangle(action,save.Size)),"Profile save action outside host");
                }
                save.PerformClick();await WaitForSave(editor);Check(saved==1&&transport.Writes==2,"Profile did not save");
                var user=await session.User.GetAsync();Check(user?.Profile.City=="Culiacán"&&user?.Profile.PhoneNumber is null&&user?.Profile.AvatarId==avatar,"Readback did not preserve update, optional clearing or avatar");
                Check(user?.Profile.DateOfBirth==new DateOnly(2003,3,15)&&user?.Profile.NationalIdentificationNumber=="TEST030315HSLSST01","Personal fields missing from update");
                using var view=new UserProfileView(user!.Value,null,null);view.Width=560;view.ArrangeView();Check(Walk(view).OfType<Label>().Any(x=>x.Text=="Culiacán"),"Saved profile value absent from view");
                foreach(int width in new[]{360,560,1080})
                {
                    view.Width=width;view.ArrangeView();var header=Walk(view).OfType<ProfileHeader>().Single();header.PerformLayout();
                    var edit=Walk(header).OfType<Button>().Single(x=>x.Text=="Editar perfil");
                    Check(header.ClientRectangle.Contains(edit.Bounds),"Edit profile action outside header");
                    Check(!header.Controls.OfType<Label>().Any(x=>x.Bounds.IntersectsWith(edit.Bounds)),"Profile text covers edit action");
                }
            }
            catch(Exception error){failure=error;}
            finally{host.Close();}
        };
        host.ShowDialog();if(failure is not null)throw failure;
        using var parent=new TestHost();using var activation=new AccountActivationForm(_=>Task.FromResult(Guid.NewGuid()));
        parent.Shown+=async(_,_)=>
        {
            try
            {
                var operation=parent.ShowPageAsync(activation);Check(!activation.TopLevel&&parent.ClientRectangle.Contains(parent.PointToClient(activation.PointToScreen(Point.Empty))),"Activation is not embedded");
                activation.Close();await operation;
            }
            catch(Exception error){failure=error;}
            finally{parent.Close();}
        };
        parent.ShowDialog();if(failure is not null)throw failure;
        Console.WriteLine("PASS profile editor: rejected update retains values, complete PUT, optional clearing, media identity, GET/readback, visible fixed actions and embedded activation. Fake HTTP only; SQL is not tested.");
    }
    private static async Task WaitForSave(UserProfileEditor editor){var deadline=DateTime.UtcNow.AddSeconds(3);while(editor.IsBusy&&DateTime.UtcNow<deadline)await Task.Delay(10);Check(!editor.IsBusy,"Update timed out");}
    private sealed class TestHost:StudioForm {internal TestHost(){ClientSize=new Size(900,700);}}
    private sealed class ProfileTransport:HttpMessageHandler
    {
        internal int Writes;private string _profile="{}";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            if(request.Method==HttpMethod.Put)
            {
                Writes++;_profile=await request.Content!.ReadAsStringAsync(token);
                return new HttpResponseMessage(Writes==1?HttpStatusCode.BadRequest:HttpStatusCode.NoContent){Content=Writes==1?new StringContent("Rechazo de prueba"):null};
            }
            return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"firstName\":\"Prueba\",\"paternalLastName\":\"Local\",\"email\":\"prueba@example.com\",\"isOwner\":true,\"profile\":"+_profile+"}",System.Text.Encoding.UTF8,"application/json")};
        }
    }
    private static IEnumerable<Control> Walk(Control root){foreach(Control child in root.Controls){yield return child;foreach(var next in Walk(child))yield return next;}}
    private static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
