namespace CoffeManager.Client.Admin.Presentation.Controls;
/// <summary>Locks editable controls during an operation while retaining readable dark labels and parent surfaces.</summary>
internal sealed class FieldBusyState : IDisposable
{
    private readonly List<Action> _restore=[];
    internal FieldBusyState(Control root)
    {
        void Lock(Control parent)
        {
            foreach(Control control in parent.Controls)
            {
                if(control is TextBox text){bool previous=text.ReadOnly;text.ReadOnly=true;_restore.Add(()=>{if(!text.IsDisposed)text.ReadOnly=previous;});}
                else if(control is Button or LinkLabel or ComboBox or CheckBox)
                {
                    bool previous=control.Enabled;control.Enabled=false;_restore.Add(()=>{if(!control.IsDisposed)control.Enabled=previous;});
                }
                else Lock(control);
            }
        }
        Lock(root);
    }
    public void Dispose(){for(int i=_restore.Count-1;i>=0;i--)_restore[i]();_restore.Clear();}
}
