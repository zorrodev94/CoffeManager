using System.Net;
using System.Text;
using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Forms;
namespace CoffeManager.PresentationChecks;
internal static class NavigationChecks
{
    internal static void Run()
    {
        using var transport=new NavigationTransport();using var connection=new ApiConnection(new Uri("https://localhost:7066"),transport);
        using var form=new AdminForm(new AdminSession(connection));Exception? failure=null;
        form.Shown+=async(_,_)=>
        {
            try
            {
                await Task.Yield();
                var buttons=Walk(form).OfType<NavigationButton>().ToArray();
                foreach(string page in new[]{"Inicio","Mi perfil","Negocio","Sucursales","Empleados"})
                {
                    var button=buttons.Single(x=>x.AccessibleName==page);button.PerformClick();await Task.Yield();
                    int calls=transport.Calls;
                    for(int i=0;i<10;i++)button.PerformClick();
                    if(transport.Calls!=calls)throw new InvalidOperationException("Repeated navigation reloaded "+page);
                }
                buttons.Single(x=>x.AccessibleName=="Mi perfil").PerformClick();await Task.Yield();
                Walk(form).OfType<Button>().Single(x=>x.Text=="Editar perfil").PerformClick();await Task.Yield();
                var editor=Walk(form).OfType<UserProfileEditor>().Single();
                var input=Walk(editor).OfType<ObsidianField>().Single(x=>x.Input.AccessibleName=="Nombre para mostrar *").Input;
                string original=input.Text;input.Text="Sin guardar";int before=transport.Calls;
                var menu=buttons.Single(x=>x.AccessibleName=="Menú");
                for(int i=0;i<20;i++){menu.PerformClick();await Task.Yield();}
                buttons.Single(x=>x.AccessibleName=="Mi perfil").PerformClick();
                if(editor.IsDisposed||input.Text!="Sin guardar"||transport.Calls!=before)throw new InvalidOperationException("Menu changes lost editor state or reloaded data");
                input.Text=original;
                // Cancel discards our deliberately dirty fixture without displaying a prompt on close.
                var cancelEvent=typeof(UserProfileEditor).GetField("_dirty",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
                cancelEvent.SetValue(editor,false);
            }
            catch(Exception error){failure=error;}
            finally
            {
                foreach(var editor in Walk(form).OfType<UserProfileEditor>())
                    typeof(UserProfileEditor).GetField("_dirty",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(editor,false);
                form.Close();
            }
        };
        form.ShowDialog();if(failure is not null)throw failure;
        Console.WriteLine("PASS navigation: repeated active-page clicks make no requests; menu changes preserve the open editor. Fake HTTP; visual repaint needs Windows review.");
    }
    private static IEnumerable<Control> Walk(Control root){foreach(Control child in root.Controls){yield return child;foreach(var nested in Walk(child))yield return nested;}}
    private sealed class NavigationTransport:HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;
            if(request.RequestUri!.AbsolutePath=="/api/users/me")return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"firstName\":\"Prueba\",\"paternalLastName\":\"Local\",\"email\":\"prueba@example.com\",\"isOwner\":true,\"profile\":{\"displayName\":\"Prueba\"}}",Encoding.UTF8,"application/json")});
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
