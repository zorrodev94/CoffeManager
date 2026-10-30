using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Data.SqlClient;
namespace CoffeManager.Infrastructure.Database;
/// <summary>Verifies actual SQL columns against the interaction structures instead of inferring persistence from a successful connection.</summary>
internal static class DatabaseVerifier
{
    #region Task Database Verification
        internal static async Task VerifyAsync(SqlConnectionFactory connections,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);
            var groups=new[]{("Accounts",new[]{typeof(AccountRegistrationData),typeof(UserProfileData)}),("Businesses",new[]{typeof(BusinessCreationData),typeof(BusinessProfileData)}),("Employees",new[]{typeof(EmployeeCreationData),typeof(EmployeeProfileData),typeof(EmployeeAssignmentData)}),("Branches",new[]{typeof(BranchCreationData),typeof(BranchProfileData)})};
            foreach(var (table,types) in groups)
            {
                using var command=connection.CreateCommand();command.CommandText="SELECT COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA=N'dbo' AND TABLE_NAME=@table";command.Text("@table",table,128);
                await using var reader=await command.ExecuteReaderAsync(cancellationToken);var columns=new Dictionary<string,(string Type,int? Length)>();while(await reader.ReadAsync(cancellationToken))columns[reader.GetString(0)]=(reader.GetString(1),reader.IsDBNull(2)?null:reader.GetInt32(2));
                foreach(var property in types.SelectMany(x=>x.GetProperties()).DistinctBy(x=>x.Name))
                {
                    if(property.Name is "Password" or "SetupKey")continue;
                    if(!columns.TryGetValue(property.Name,out var column))throw new InvalidOperationException($"Falta dbo.{table}.{property.Name}. Ejecuta --initialize-database.");
                    var type=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;var expected=type==typeof(string)?"nvarchar":type==typeof(Guid)?"uniqueidentifier":type==typeof(DateOnly)?"date":type==typeof(decimal)?"decimal":"";
                    if(column.Type!=expected)throw new InvalidOperationException($"Tipo SQL incompatible en dbo.{table}.{property.Name}.");
                    if(property.GetCustomAttribute<StringLengthAttribute>() is {} length&&column.Length!=length.MaximumLength)throw new InvalidOperationException($"Longitud SQL incompatible en dbo.{table}.{property.Name}.");
                }
                Console.WriteLine($"VERIFICADO dbo.{table}: {columns.Count} columnas; campos de registro y perfil compatibles.");
            }
            using var versions=connection.CreateCommand();versions.CommandText="SELECT COUNT(*) FROM dbo.SchemaVersions WHERE Version IN(1,2)";
            if(Convert.ToInt32(await versions.ExecuteScalarAsync(cancellationToken))!=2)throw new InvalidOperationException("El esquema no contiene las versiones 1 y 2.");
            using var catalogs=connection.CreateCommand();catalogs.CommandText="SELECT COUNT(*) FROM dbo.InitializationHistory WHERE Name=N'ProfileCatalogsV2'";
            if(Convert.ToInt32(await catalogs.ExecuteScalarAsync(cancellationToken))!=1)throw new InvalidOperationException("Los catálogos iniciales no están preparados.");
            Console.WriteLine("VERIFICADO: versiones 1 y 2, catálogos y estructura de persistencia.");
        }
    #endregion Task Database Verification
}
