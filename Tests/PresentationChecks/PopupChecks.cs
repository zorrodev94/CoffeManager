using CoffeManager.Client.Admin.Presentation.Controls;
namespace CoffeManager.PresentationChecks;
internal static class PopupChecks
{
    internal static void Run()
    {
        Exception? failure=null;using var host=new Form{ClientSize=new Size(700,500)};
        var selectors=new[]{new ObsidianSelector("Empleado"),new ObsidianSelector("Sucursal"),new ObsidianSelector("Rol")};
        for(int i=0;i<selectors.Length;i++){selectors[i].SetBounds(30,30+i*80,500,52);selectors[i].Input.Items.AddRange(["Sin asignación","Opción uno","Opción dos"]);selectors[i].Input.SelectedIndex=0;host.Controls.Add(selectors[i]);}
        host.Shown+=async(_,_)=>
        {
            try
            {
                foreach(var selector in selectors)
                {
                    var input=(ObsidianComboBox)selector.Input;
                    for(int i=0;i<5;i++)
                    {
                        input.OpenOptions();var popup=input.OptionsPopup??throw new Exception("Popup did not open");
                        var row=Walk(popup).OfType<Button>().Single(x=>x.Text=="Opción dos");row.PerformClick();
                        if(input.SelectedIndex!=2)throw new Exception("Selection lost");
                        if(popup.IsDisposed)throw new Exception("Popup disposed inside synchronous close");
                        await Task.Delay(20);if(!popup.IsDisposed)throw new Exception("Retired popup leaked");
                        input.OpenOptions();input.OptionsPopup!.Close();input.OpenOptions();
                        await Task.Delay(20);if(input.OptionsPopup is null||input.OptionsPopup.IsDisposed)throw new Exception("Retirement affected reopened popup");
                        input.OptionsPopup.Close();await Task.Delay(20);
                    }
                }
                selectors[0].Input.SelectedIndex=1;
                var last=(ObsidianComboBox)selectors[0].Input;last.OpenOptions();last.OptionsPopup!.Close();selectors[0].Dispose();await Task.Delay(20);
            }
            catch(Exception error){failure=error;}
            finally{host.Close();}
        };
        host.ShowDialog();if(failure is not null)throw failure;
        Console.WriteLine("PASS popup lifecycle: three selectors, repeated selection, cancellation, reopening before deferred disposal and owner disposal. No API/SQL.");
    }
    private static IEnumerable<Control> Walk(Control root){foreach(Control child in root.Controls){yield return child;foreach(var next in Walk(child))yield return next;}}
}
