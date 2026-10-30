using Microsoft.Data.SqlClient;
namespace CoffeManager.Infrastructure.Database;
/// <summary>
/// Sql connection factory for the current entity operation.
/// </summary>
/// <param name="configuration">The configuration used by this operation.</param>
internal sealed class SqlConnectionFactory(IConfiguration configuration)
{
    #region Connection Operations
        /// <summary>Opens an independently owned SQL connection for one repository operation.</summary>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
        {
            var value = configuration.GetConnectionString("CoffeManager") ?? throw new InvalidOperationException("Configure ConnectionStrings:CoffeManager.");
            var connection = new SqlConnection(value);
            try { await connection.OpenAsync(cancellationToken); return connection; }
            catch { await connection.DisposeAsync(); throw; }
        }
    #endregion Connection Operations
}
