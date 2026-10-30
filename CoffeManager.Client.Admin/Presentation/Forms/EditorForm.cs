using CoffeManager.Client.Admin.Presentation.Windows;
using System.ComponentModel.DataAnnotations;
using CoffeManager.Client.Admin.Presentation.Theme;
using CoffeManager.Client.Admin.Presentation.Controls;
namespace CoffeManager.Client.Admin.Presentation.Forms;
/// <summary>Hosts compact modal forms without coupling field layout to persistence.</summary>
internal sealed class EditorForm : StudioForm
{
    #region Operation Data
        private readonly FlowLayoutPanel _fields;
        private Func<object>? _validationData;
        private int _pendingOperations;
    #endregion Operation Data
    #region Initialization
        /// <summary>Creates a scrollable editor with explicit confirmation.</summary>
        /// <param name="title">The title used by this operation.</param>
        internal EditorForm(string title)
        {
            Text = title; Width = 760; Height = 820; MinimumSize = new Size(520, 450);
            StartPosition = FormStartPosition.CenterParent; BackColor = MarfilCafe.Ivory;
            Font = new Font("Segoe UI", 11); AutoScaleDimensions = new SizeF(96F,96F); AutoScaleMode = AutoScaleMode.Dpi; 
            var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 70, Padding = new Padding(20, 5, 0, 0) };
            var save = MarfilCafe.Action("Guardar", (_, _) =>
            {
                if(_pendingOperations>0){MessageBox.Show(this,"Espera a que termine la carga de la imagen antes de guardar.","Preparando imagen");return;}
                if(_validationData is not null)
                {
                    object data;
                    try{data=_validationData();}
                    catch(ValidationException error){MessageBox.Show(this,error.Message,"Revisa los datos",MessageBoxButtons.OK,MessageBoxIcon.Information);return;}
                    var errors=new List<ValidationResult>();
                    if(!Validator.TryValidateObject(data,new ValidationContext(data),errors,true))
                    {
                        MessageBox.Show(this,"Revisa los datos antes de guardar:\n"+string.Join("\n",errors.Select(x=>x.ErrorMessage)),"Datos incompletos",MessageBoxButtons.OK,MessageBoxIcon.Information);return;
                    }
                }
                DialogResult=DialogResult.OK;Close();
            });
            var cancel = MarfilCafe.Action("Cancelar", (_, _) => { DialogResult = DialogResult.Cancel; Close(); }, false);
            actions.Controls.AddRange([save, cancel]); AcceptButton = save; CancelButton = cancel;
            _fields = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(24) };
            _fields.Controls.Add(MarfilCafe.Text(title, 20, true));
            Body.Controls.Add(_fields); Body.Controls.Add(actions);
            void Reflow()
            {
                var width = Math.Max(180, _fields.ClientSize.Width - _fields.Padding.Horizontal - 25);
                foreach(Control control in _fields.Controls)
                {
                    if(control is TextBox||control is StudioSurface||control is ComboBox||control is DateTimePicker||control is NumericUpDown) control.Width = width;
                    if(control is Label label) label.MaximumSize = new Size(width, 0);
                    if(control is FlowLayoutPanel row)
                    {
                        row.MaximumSize=new Size(width,0);row.Width=width;
                        foreach(Control child in row.Controls)if(child is PictureBox picture&&picture.Width>120)picture.Width=Math.Min(width,420);
                    }
                }
            }
            _fields.Resize+=(_,_)=>Reflow();_fields.ControlAdded+=(_,_)=>Reflow();Shown+=(_,_)=>Reflow();
        }
    #endregion Initialization
    #region Field Operations
        /// <summary>Prevents confirmation while selected media are still being uploaded.</summary>
        internal void BeginPendingOperation()=>_pendingOperations++;
        internal void EndPendingOperation()=>_pendingOperations=Math.Max(0,_pendingOperations-1);
        /// <summary>Builds and validates the interaction data before confirmation.</summary>
        /// <param name="data">Builds the complete request from the current field values.</param>
        internal void ValidateBeforeConfirm(Func<object> data)=>_validationData=data;
        /// <summary>Adds a labelled input whose value remains available after validation errors.</summary>
        internal TextBox AddField(string label, string value = "", bool multiline = false)
        {
            _fields.Controls.Add(MarfilCafe.Text(label));
            var input = MarfilCafe.Input(value, multiline);
            var surface=new StudioSurface{Width=420,Height=multiline?122:54,Margin=new Padding(0,0,0,14)};
            input.Dock=DockStyle.Fill;surface.Controls.Add(input);input.Enter+=(_,_)=>surface.Invalidate();input.Leave+=(_,_)=>surface.Invalidate();
            _fields.Controls.Add(surface); return input;
        }
        /// <summary>Adds a section heading inside the current interaction form.</summary>
        internal void AddSection(string title)=>_fields.Controls.Add(MarfilCafe.Text(title,15,true));
        internal void AddEditor(string label,Control control)
        {
            _fields.Controls.Add(MarfilCafe.Text(label));control.Width=620;control.Margin=new Padding(0,0,0,14);_fields.Controls.Add(control);
        }
        /// <summary>Adds supporting information without making it editable.</summary>
        internal void AddNote(string text) => _fields.Controls.Add(MarfilCafe.Text(text));
    #endregion Field Operations
}
