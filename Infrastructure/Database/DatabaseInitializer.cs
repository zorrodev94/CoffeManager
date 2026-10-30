using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
namespace CoffeManager.Infrastructure.Database;
/// <summary>Applies versioned schema changes and seeds catalogs only through the explicit initialization command.</summary>
internal static class DatabaseInitializer
{
    #region Task Database Initialization
        /// <summary>Applies ordered scripts, then initializes persisted defaults in one guarded transaction.</summary>
        internal static async Task InitializeAsync(SqlConnectionFactory connections,string root,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);
            foreach(var file in Directory.GetFiles(Path.Combine(root,"Infrastructure","Database","Scripts"),"*.sql").Where(x=>!Path.GetFileName(x).StartsWith("000_")).OrderBy(x=>x,StringComparer.Ordinal))
            {
                using var command=connection.CreateCommand();command.CommandText=await File.ReadAllTextAsync(file,cancellationToken);command.CommandTimeout=120;await command.ExecuteNonQueryAsync(cancellationToken);
            }
            await using var transaction=(SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable,cancellationToken);
            using var seed=connection.CreateCommand();seed.Transaction=transaction;
            seed.CommandText="SELECT COUNT(*) FROM dbo.InitializationHistory WITH(UPDLOCK,HOLDLOCK) WHERE Name=N'ProfileCatalogsV2'";
            if(Convert.ToInt32(await seed.ExecuteScalarAsync(cancellationToken))==0)
            {
                seed.CommandText="""
                    INSERT dbo.Roles(RoleId,Code,Name,CanAccessAdministration,CanAccessCheckout)
                    SELECT NEWID(),value,CASE value WHEN N'Admin' THEN N'Administrador' WHEN N'Manager' THEN N'Encargado' WHEN N'Cashier' THEN N'Cajero' ELSE N'Empleado' END,
                    CASE WHEN value IN(N'Admin',N'Manager') THEN 1 ELSE 0 END,CASE WHEN value IN(N'Admin',N'Manager',N'Cashier') THEN 1 ELSE 0 END FROM OPENJSON(@roles);
                    INSERT dbo.SalaryBases(Id,Code,Name)
                    SELECT NEWID(),value,CASE value WHEN N'Hourly' THEN N'Por hora' WHEN N'Daily' THEN N'Diario' WHEN N'Weekly' THEN N'Semanal' WHEN N'Biweekly' THEN N'Cada dos semanas' WHEN N'Semimonthly' THEN N'Quincenal' ELSE N'Mensual' END FROM OPENJSON(@salary);
                    INSERT dbo.EmploymentStatuses(Id,Code,Name)
                    SELECT NEWID(),value,CASE value WHEN N'Pending' THEN N'Pendiente' WHEN N'Active' THEN N'Activo' WHEN N'OnLeave' THEN N'Permiso temporal' WHEN N'Suspended' THEN N'Suspendido' ELSE N'Finalizado' END FROM OPENJSON(@statuses);
                    INSERT dbo.InitializationHistory(Name) VALUES(N'ProfileCatalogsV2');
                    """;
                seed.Text("@roles",JsonSerializer.Serialize(CoffeManager.Models.User.Base.User.GetInitializationRoles()),-1);
                var defaults=CoffeManager.Models.User.Classes.EmployeeUser.GetInitializationCatalogs();seed.Text("@salary",JsonSerializer.Serialize(defaults.Salary),-1);seed.Text("@statuses",JsonSerializer.Serialize(defaults.Statuses),-1);
                await seed.ExecuteNonQueryAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            using var verify=connection.CreateCommand();verify.CommandText="SELECT Version FROM dbo.SchemaVersions ORDER BY Version";
            await using var reader=await verify.ExecuteReaderAsync(cancellationToken);var versions=new List<int>();while(await reader.ReadAsync(cancellationToken))versions.Add(reader.GetInt32(0));
            Console.WriteLine("Esquema verificado. Versiones aplicadas: "+string.Join(", ",versions)+". No se eliminaron cuentas ni datos.");
        }
    #endregion Task Database Initialization
}
