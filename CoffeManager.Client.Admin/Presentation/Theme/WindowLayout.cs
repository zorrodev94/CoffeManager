namespace CoffeManager.Client.Admin.Presentation.Theme;
/// <summary>Keeps windows inside the current monitor while scrollable containers retain access to their content.</summary>
internal static class WindowLayout
{
    #region Window Operations
        /// <summary>Registers viewport adjustment for startup and monitor DPI changes.</summary>
        /// <param name="form">The form whose working area must be respected.</param>
        internal static void Configure(Form form)
        {
            form.Shown += (_, _) => Fit(form);
            form.DpiChanged += (_, _) => Fit(form);
        }
        private static void Fit(Form form)
        {
            var area = Screen.FromControl(form).WorkingArea;
            var width = Math.Max(200, area.Width - 24);
            var height = Math.Max(200, area.Height - 24);
            form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, width), Math.Min(form.MinimumSize.Height, height));
            if(form.WindowState != FormWindowState.Normal) return;
            form.Size = new Size(Math.Min(form.Width, width), Math.Min(form.Height, height));
            form.Location = new Point(Math.Clamp(form.Left, area.Left, area.Right - form.Width), Math.Clamp(form.Top, area.Top, area.Bottom - form.Height));
        }
    #endregion Window Operations
}
