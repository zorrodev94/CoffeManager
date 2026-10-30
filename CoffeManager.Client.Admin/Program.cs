using CoffeManager.Client.Admin.Connection;
using CoffeManager.Client.Admin.Presentation.Forms;
namespace CoffeManager.Client.Admin;
internal static class Program
{
    #region Application Startup
        /// <summary>Starts the administrative client using the configured local API.</summary>
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();
            try
            {
                var address=Environment.GetEnvironmentVariable("COFFEMANAGER_API_URL")??"https://localhost:7066/";
                using var connection=new ApiConnection(new Uri(address));
                Application.Run(new AccessForm(new AdminSession(connection)));
            }
            catch(Exception error) when(error is ArgumentException or UriFormatException)
            { MessageBox.Show(error.Message,"Configuración del cliente"); }
        }
    #endregion Application Startup
}
