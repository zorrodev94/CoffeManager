using CoffeManager.Client.Admin.Presentation.Controls;
namespace CoffeManager.Client.Admin.Presentation.Theme;
internal static class MarfilCafe
{
    #region Theme Data
        internal static readonly Color Ivory = Color.FromArgb(249, 245, 237);
        internal static readonly Color Espresso = Color.FromArgb(57, 35, 24);
        internal static readonly Color Amber = Color.FromArgb(177, 91, 28);
        internal static readonly Color Muted = Color.FromArgb(112, 97, 84);
    #endregion Theme Data
    #region Control Factories
        /// <summary>Creates an automatically sized label with consistent typography.</summary>
        /// <param name="text">The text used by this operation.</param>
        /// <param name="size">The size used by this operation.</param>
        /// <param name="bold">The bold used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal static Label Text(string text, float size = 11, bool bold = false) => new()
        {
            Text = text, AutoSize = true, ForeColor = Espresso,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            Margin = new Padding(0, 8, 0, 12), MaximumSize = new Size(760, 0)
        };
        /// <summary>Creates an accessible, automatically sized action button.</summary>
        /// <param name="text">The text used by this operation.</param>
        /// <param name="handler">The handler used by this operation.</param>
        /// <param name="primary">The primary used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal static Button Action(string text, EventHandler handler, bool primary = true)
        {
            var button = new StudioButton(primary)
            {
                Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(130, 42), Padding = new Padding(14, 6, 14, 6),
                FlatStyle = FlatStyle.Flat, BackColor = primary ? Amber : Ivory,
                ForeColor = primary ? Color.White : Espresso, Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 12, 8), AccessibleName = text
            };
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.Click += handler;
            return button;
        }
        /// <summary>Creates a consistently spaced input with a visible external label.</summary>
        /// <param name="value">The value used by this operation.</param>
        /// <param name="multiline">The multiline used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal static TextBox Input(string value = "", bool multiline = false) => new()
        {
            BorderStyle = BorderStyle.None, BackColor = Color.White, ForeColor = Espresso, Text = value, Width = 420, Multiline = multiline, Height = multiline ? 90 : 32,
            MaxLength = multiline ? 1000 : 160, Margin = new Padding(0, 0, 0, 10)
        };
    #endregion Control Factories
}
