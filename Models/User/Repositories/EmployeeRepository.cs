using System.Data;
using CoffeManager.Infrastructure.Database;
using static CoffeManager.Infrastructure.Database.MediaOwnership;
namespace CoffeManager.Models.User.Repositories;
/// <summary>Persists employee identity, complete profiles and authorized assignments.</summary>
internal sealed partial class EmployeeRepository(SqlConnectionFactory connections)
{
    #region Task Employee Operations
        /// <summary>Queries employees within the authenticated owner's scope.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<IReadOnlyList<EmployeeDetailsData>> GetAsync(Guid ownerId,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="SELECT EmployeeId,CreatedAt,FirstName,PaternalLastName,MaternalLastName,BranchId,RoleId,UserAccountId,Email,DisplayName,Description,AvatarId,CoverImageId,PhoneNumber,AlternatePhoneNumber,DateOfBirth,Address,City,State,PostalCode,TaxIdentificationNumber,NationalIdentificationNumber,EmployeeNumber,HireDate,EmploymentStatusId,SalaryAmount,SalaryBasisId FROM dbo.Employees WHERE OwnerUserId=@owner ORDER BY FirstName,EmployeeId";command.Id("@owner",ownerId);
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);var items=new List<EmployeeDetailsData>();
            while(await reader.ReadAsync(cancellationToken))items.Add(new EmployeeDetailsData{EmployeeId=reader.GetGuid(0),CreatedAt=reader.GetDateTimeOffset(1),Identity=new EmployeeCreationData{FirstName=reader.GetString(2),PaternalLastName=reader.GetString(3),MaternalLastName=reader.Optional(4)},Assignment=new EmployeeAssignmentData{BranchId=reader.IsDBNull(5)?null:reader.GetGuid(5),RoleId=reader.IsDBNull(6)?null:reader.GetGuid(6),UserAccountId=reader.IsDBNull(7)?null:reader.GetGuid(7)},Profile=new EmployeeProfileData{Email=reader.IsDBNull(8)?null:reader.GetString(8),DisplayName=reader.IsDBNull(9)?null:reader.GetString(9),Description=reader.IsDBNull(10)?null:reader.GetString(10),AvatarId=reader.IsDBNull(11)?null:reader.GetGuid(11),CoverImageId=reader.IsDBNull(12)?null:reader.GetGuid(12),PhoneNumber=reader.IsDBNull(13)?null:reader.GetString(13),AlternatePhoneNumber=reader.IsDBNull(14)?null:reader.GetString(14),DateOfBirth=reader.IsDBNull(15)?null:DateOnly.FromDateTime(reader.GetDateTime(15)),Address=reader.IsDBNull(16)?null:reader.GetString(16),City=reader.IsDBNull(17)?null:reader.GetString(17),State=reader.IsDBNull(18)?null:reader.GetString(18),PostalCode=reader.IsDBNull(19)?null:reader.GetString(19),TaxIdentificationNumber=reader.IsDBNull(20)?null:reader.GetString(20),NationalIdentificationNumber=reader.IsDBNull(21)?null:reader.GetString(21),EmployeeNumber=reader.IsDBNull(22)?null:reader.GetString(22),HireDate=reader.IsDBNull(23)?null:DateOnly.FromDateTime(reader.GetDateTime(23)),EmploymentStatusId=reader.IsDBNull(24)?null:reader.GetGuid(24),SalaryAmount=reader.IsDBNull(25)?null:reader.GetDecimal(25),SalaryBasisId=reader.IsDBNull(26)?null:reader.GetGuid(26)}});return items;
        }
        /// <summary>Creates a minimal employee record without an account or implicit permissions.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<Guid> CreateAsync(Guid ownerId,EmployeeCreationData data,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();var id=Guid.NewGuid();
            command.CommandText="INSERT dbo.Employees(EmployeeId,OwnerUserId,FirstName,PaternalLastName,MaternalLastName) VALUES(@id,@owner,@first,@last,@second)";
            command.Id("@id",id);command.Id("@owner",ownerId);command.Text("@first",data.FirstName.Trim(),80);command.Text("@last",data.PaternalLastName.Trim(),80);command.Text("@second",data.MaternalLastName,80);
            await command.ExecuteNonQueryAsync(cancellationToken);return id;
        }
        /// <summary>Saves all additional employee fields using bounded SQL parameters.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="employeeId">The selected employee identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task UpdateAsync(Guid ownerId,Guid employeeId,EmployeeProfileData data,CancellationToken cancellationToken)
        {
            if(data.DateOfBirth>DateOnly.FromDateTime(DateTime.Today))throw new ArgumentException("La fecha de nacimiento no puede ser futura.");
            if(data.SalaryAmount is not null&&data.SalaryBasisId is null)throw new ArgumentException("Selecciona la periodicidad del salario.");
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="UPDATE dbo.Employees SET Email=@Email,DisplayName=@DisplayName,Description=@Description,AvatarId=@AvatarId,CoverImageId=@CoverImageId,PhoneNumber=@PhoneNumber,AlternatePhoneNumber=@AlternatePhoneNumber,DateOfBirth=@DateOfBirth,Address=@Address,City=@City,State=@State,PostalCode=@PostalCode,TaxIdentificationNumber=@TaxIdentificationNumber,NationalIdentificationNumber=@NationalIdentificationNumber,EmployeeNumber=@EmployeeNumber,HireDate=@HireDate,EmploymentStatusId=@EmploymentStatusId,SalaryAmount=@SalaryAmount,SalaryBasisId=@SalaryBasisId WHERE OwnerUserId=@owner AND EmployeeId=@id";
            command.Id("@owner",ownerId);command.Id("@id",employeeId);command.Text("@Email",data.Email,254);
            command.Text("@DisplayName",data.DisplayName,100);
            command.Text("@Description",data.Description,1000);
            command.Parameters.Add("@AvatarId",SqlDbType.UniqueIdentifier).Value=(object?)data.AvatarId??DBNull.Value;
            command.Parameters.Add("@CoverImageId",SqlDbType.UniqueIdentifier).Value=(object?)data.CoverImageId??DBNull.Value;
            command.Text("@PhoneNumber",data.PhoneNumber,30);
            command.Text("@AlternatePhoneNumber",data.AlternatePhoneNumber,30);
            command.Parameters.Add("@DateOfBirth",SqlDbType.Date).Value=(object?)data.DateOfBirth?.ToDateTime(TimeOnly.MinValue)??DBNull.Value;
            command.Text("@Address",data.Address,250);
            command.Text("@City",data.City,100);
            command.Text("@State",data.State,100);
            command.Text("@PostalCode",data.PostalCode,10);
            command.Text("@TaxIdentificationNumber",data.TaxIdentificationNumber,13);
            command.Text("@NationalIdentificationNumber",data.NationalIdentificationNumber,18);
            command.Text("@EmployeeNumber",data.EmployeeNumber,30);
            command.Parameters.Add("@HireDate",SqlDbType.Date).Value=(object?)data.HireDate?.ToDateTime(TimeOnly.MinValue)??DBNull.Value;
            command.Parameters.Add("@EmploymentStatusId",SqlDbType.UniqueIdentifier).Value=(object?)data.EmploymentStatusId??DBNull.Value;
            var salaryAmount=command.Parameters.Add("@SalaryAmount",SqlDbType.Decimal);salaryAmount.Precision=18;salaryAmount.Scale=2;salaryAmount.Value=(object?)data.SalaryAmount??DBNull.Value;
            command.Parameters.Add("@SalaryBasisId",SqlDbType.UniqueIdentifier).Value=(object?)data.SalaryBasisId??DBNull.Value;
            await ValidateMediaAsync(command,ProfileMedia(data.AvatarId,data.CoverImageId),ownerId,cancellationToken);
            if(await command.ExecuteNonQueryAsync(cancellationToken)!=1)throw new KeyNotFoundException();
        }
    #endregion Task Employee Operations
}
