using System.Data;
using Microsoft.Data.SqlClient;
namespace CoffeManager.Infrastructure.Database;
internal static class SqlParameters
{
    /// <summary>
    /// Id for the current entity operation.
    /// </summary>
    /// <param name="command">The command used by this operation.</param>
    /// <param name="name">The name used by this operation.</param>
    /// <param name="value">The value used by this operation.</param>
    internal static void Id(this SqlCommand command, string name, Guid value) => command.Parameters.Add(name, SqlDbType.UniqueIdentifier).Value = value;
    /// <summary>
    /// Text for the current entity operation.
    /// </summary>
    /// <param name="command">The command used by this operation.</param>
    /// <param name="name">The name used by this operation.</param>
    /// <param name="value">The value used by this operation.</param>
    /// <param name="size">The size used by this operation.</param>
    internal static void Text(this SqlCommand command, string name, string? value, int size) => command.Parameters.Add(name, SqlDbType.NVarChar, size).Value = (object?)value ?? DBNull.Value;
    /// <summary>
    /// Optional for the current entity operation.
    /// </summary>
    /// <param name="reader">The reader used by this operation.</param>
    /// <param name="index">The index used by this operation.</param>
    /// <returns>The result of the requested operation.</returns>
    internal static string? Optional(this SqlDataReader reader, int index) => reader.IsDBNull(index) ? null : reader.GetString(index);
}
