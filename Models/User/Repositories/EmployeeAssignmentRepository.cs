using System.Data;
using Microsoft.Data.SqlClient;
using CoffeManager.Infrastructure.Database;
namespace CoffeManager.Models.User.Repositories;
internal sealed partial class EmployeeRepository
{
    #region Task Assignment Operations
        /// <summary>Locks the employee and candidate account while validating an operational assignment.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="employeeId">The selected employee identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task AssignAsync(Guid ownerId,Guid employeeId,EmployeeAssignmentData data,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);
            await using var transaction=(SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable,cancellationToken);
            using var command=connection.CreateCommand();command.Transaction=transaction;
            command.CommandText="SELECT UserAccountId FROM dbo.Employees WITH(UPDLOCK,HOLDLOCK) WHERE EmployeeId=@id AND OwnerUserId=@owner";command.Id("@id",employeeId);command.Id("@owner",ownerId);
            var previous=await command.ExecuteScalarAsync(cancellationToken);if(previous is null)throw new KeyNotFoundException();
            if(data.BranchId is Guid branchId)
            {
                command.CommandText="SELECT COUNT(*) FROM dbo.Branches s INNER JOIN dbo.Businesses b ON s.BusinessId=b.BusinessId WHERE s.BranchId=@branch AND b.OwnerUserId=@owner";command.Id("@branch",branchId);
                if(Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken))!=1)throw new UnauthorizedAccessException();
            }
            bool needsAccount=false;
            if(data.RoleId is Guid roleId)
            {
                command.CommandText="SELECT CAST(CASE WHEN CanAccessAdministration=1 OR CanAccessCheckout=1 THEN 1 ELSE 0 END AS bit) FROM dbo.Roles WHERE RoleId=@role";command.Id("@role",roleId);
                var required=await command.ExecuteScalarAsync(cancellationToken);if(required is null)throw new ArgumentException("El rol seleccionado ya no está disponible.");needsAccount=(bool)required;
            }
            if(needsAccount&&(data.UserAccountId is null||data.BranchId is null))throw new ArgumentException("Este rol requiere una cuenta registrada y una sucursal asignada.");
            if(data.UserAccountId is Guid accountId)
            {
                command.CommandText="SELECT COUNT(*) FROM dbo.Accounts a WITH(UPDLOCK,HOLDLOCK) WHERE a.UserId=@account AND a.OwnerUserId=@owner AND a.IsOwner=0 AND a.IsEnabled=1 AND NOT EXISTS(SELECT 1 FROM dbo.Employees e WITH(UPDLOCK,HOLDLOCK) WHERE e.UserAccountId=a.UserId AND e.EmployeeId<>@id)";command.Id("@account",accountId);
                if(Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken))!=1)throw new ArgumentException("La cuenta ya está vinculada o no está disponible para este empleado.");
            }
            command.CommandText="UPDATE dbo.Employees SET BranchId=@newBranch,RoleId=@newRole,UserAccountId=@newAccount WHERE EmployeeId=@id AND OwnerUserId=@owner";
            command.Parameters.Add("@newBranch",SqlDbType.UniqueIdentifier).Value=(object?)data.BranchId??DBNull.Value;command.Parameters.Add("@newRole",SqlDbType.UniqueIdentifier).Value=(object?)data.RoleId??DBNull.Value;command.Parameters.Add("@newAccount",SqlDbType.UniqueIdentifier).Value=(object?)data.UserAccountId??DBNull.Value;await command.ExecuteNonQueryAsync(cancellationToken);
            command.CommandText="UPDATE dbo.Accounts SET AuthVersion=AuthVersion+1 WHERE UserId=@old OR UserId=@newAccount";command.Parameters.Add("@old",SqlDbType.UniqueIdentifier).Value=previous;await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    #endregion Task Assignment Operations
    #region Task Catalog Operations
        /// <summary>Maps only approved catalog keys to SQL identifiers.</summary>
        private static string CatalogTable(string catalog)=>catalog switch{"salary-bases"=>"SalaryBases","employment-statuses"=>"EmploymentStatuses",_=>throw new ArgumentException("Catálogo no disponible.")};
        internal async Task<IReadOnlyList<CatalogData>> GetCatalogAsync(string catalog,CancellationToken cancellationToken)
        {
            var table=CatalogTable(catalog);await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();command.CommandText=$"SELECT Id,Code,Name FROM dbo.{table} ORDER BY Name";
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);var result=new List<CatalogData>();while(await reader.ReadAsync(cancellationToken))result.Add(new CatalogData{Id=reader.GetGuid(0),Code=reader.GetString(1),Name=reader.GetString(2)});return result;
        }
        internal async Task<Guid> AddCatalogOptionAsync(string catalog,CatalogCreationData data,CancellationToken cancellationToken)
        {
            var table=CatalogTable(catalog);await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();var id=Guid.NewGuid();command.CommandText=$"INSERT dbo.{table}(Id,Code,Name) VALUES(@id,@code,@name)";command.Id("@id",id);command.Text("@code",id.ToString("N"),50);command.Text("@name",data.Name.Trim(),100);await command.ExecuteNonQueryAsync(cancellationToken);return id;
        }
    #endregion Task Catalog Operations
}
