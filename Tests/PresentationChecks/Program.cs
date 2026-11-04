using CoffeManager.Client.Admin.Presentation.Controls;
using CoffeManager.Client.Admin.Presentation.Forms;
namespace CoffeManager.PresentationChecks;
internal static class Program
{
    /// <summary>Exercises the real activation form using a fake submission callback; never contacts the API or SQL.</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
        if(args.Contains("--preview-activation"))
        {
            using var preview=new AccountActivationForm(_=>Task.FromException<Guid>(new InvalidOperationException("Vista previa: no se enviaron datos ni se creó una cuenta."))){Text="VISTA PREVIA · Primera activación · Sin conexión"};
            preview.ShowDialog();return;
        }
        using(var emptyView=new UserProfileView(new(){FirstName="Prueba",PaternalLastName="Local",Email="prueba@example.com",Profile=new(){DisplayName="Prueba"}},null,null)){emptyView.Width=560;emptyView.ArrangeView();emptyView.Width=1100;emptyView.ArrangeView();}
        PopupChecks.Run();
        NavigationChecks.Run();
        int calls=0;Exception? failure=null;
        using var form=new AccountActivationForm(data=>{calls++;return calls==1?Task.FromException<Guid>(new InvalidOperationException("Rechazo de prueba")):Task.FromResult(Guid.NewGuid());});
        form.Shown+=(_,_)=>form.BeginInvoke((Action)(()=>
        {
            try
            {
                var fields=Walk(form).OfType<ObsidianField>().ToArray();
                ObsidianField Field(string label)=>fields.Single(x=>x.Input.AccessibleName==label);
                var submit=(Button)form.AcceptButton!;
                submit.PerformClick();Assert(calls==0,"Blank activation submitted");
                Field("Nombre(s)").Input.Text="Prueba";Field("Apellido paterno").Input.Text="Local";Field("Correo electrónico").Input.Text="prueba@example.com";
                Field("Contraseña").Input.Text="Password-Prueba-123";Field("Confirmar contraseña").Input.Text="Diferente";Field("Clave de activación").Input.Text=new string('A',64);
                submit.PerformClick();Assert(calls==0,"Mismatching confirmation submitted");
                Field("Confirmar contraseña").Input.Text="Password-Prueba-123";submit.PerformClick();
                Assert(calls==1&&form.DialogResult!=DialogResult.OK,"Rejected callback closed activation");
                Assert(Field("Nombre(s)").Input.Text=="Prueba"&&Field("Clave de activación").Input.Text.Length==64,"Rejected request lost form data");
                foreach(var size in new[]{new Size(1100,800),new Size(680,620)})
                {
                    form.Size=size;form.PerformLayout();
                    foreach(var field in fields){field.PerformLayout();Assert(field.Input.BorderStyle==BorderStyle.None,"Native field border enabled");Assert(field.Input.Left>=0&&field.Input.Right<=field.Width,"Text input outside field");}
                    var position=form.PointToClient(submit.PointToScreen(Point.Empty));Assert(new Rectangle(Point.Empty,form.ClientSize).Contains(new Rectangle(position,submit.Size)),"Submit action outside window");
                }
                using var panel=new Panel();using var label=new Label{Text="Readable"};using var input=new TextBox();panel.Controls.AddRange([label,input]);
                using(new FieldBusyState(panel)){Assert(label.Enabled&&input.ReadOnly,"Busy state disabled label or left text editable");}Assert(!input.ReadOnly,"Busy state did not restore input");
                submit.PerformClick();Assert(calls==2&&form.DialogResult==DialogResult.OK,"Valid activation did not finish");
            }
            catch(Exception error){failure=error;form.Close();}
        }));
        form.ShowDialog();if(failure is not null)throw failure;
        using var connection=new CoffeManager.Client.Admin.Connection.ApiConnection(new Uri("https://localhost:7066"));
        var session=new CoffeManager.Client.Admin.Connection.AdminSession(connection);
        using var staff=new StaffAccountForm(session,[],[],[]);
        staff.Shown+=(_,_)=>staff.BeginInvoke((Action)(()=>
        {
            try
            {
                var selectors=Walk(staff).OfType<ObsidianSelector>().ToArray();Assert(selectors.Length==3&&selectors.All(x=>x.Visible),"Assignment selectors disappeared when unlinked");
                Assert(selectors.All(x=>!x.Input.Enabled),"Unlinked assignment selectors are editable");
                var link=Walk(staff).OfType<ObsidianCheckBox>().Single();link.Checked=true;
                using(new FieldBusyState(staff)){}
                Assert(selectors.All(x=>Walk(x).OfType<Button>().All(b=>b.Enabled)),"Selector arrow was not restored after busy state");
                Assert(selectors.All(x=>x.Visible&&x.Input.Enabled),"Linked selectors did not enable");
            }
            catch(Exception error){failure=error;}
            finally{staff.Close();}
        }));
        staff.ShowDialog();if(failure is not null)throw failure;
        ProfileChecks.Run();
        Console.WriteLine("PASS real WinForms activation: required fields, confirmation, rejection retains data, successful callback, fixed actions, field bounds and busy-state restoration. Role/branch/employee selectors remain visible in both linking states. No API/SQL requests.");
    }
    private static IEnumerable<Control> Walk(Control root){foreach(Control child in root.Controls){yield return child;foreach(var next in Walk(child))yield return next;}}
    private static void Assert(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
