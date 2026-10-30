using System.Data;
using static CoffeManager.Infrastructure.Database.MediaOwnership;
using CoffeManager.Infrastructure.Database;
namespace CoffeManager.Models.User.Repositories;
/// <summary>Persists account identity, credentials and complete private profiles.</summary>
/// <param name="connections">The connections used by this operation.</param>
internal sealed partial class AccountRepository(SqlConnectionFactory connections)
{
    #region Account Operations
        /// <summary>Creates the only initial owner atomically; setup cannot create a second owner.</summary>
        /// <param name="data">The data used by this operation.</param>
        /// <param name="passwordHash">The password hash used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<Guid?> CreateOwnerAsync(AccountRegistrationData data, string passwordHash, CancellationToken cancellationToken)
        {
            await using var connection = await connections.OpenAsync(cancellationToken);
            await using var transaction = (Microsoft.Data.SqlClient.SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            using var command = connection.CreateCommand(); command.Transaction = transaction;
            command.CommandText = "SELECT COUNT(*) FROM dbo.Accounts WITH (UPDLOCK,HOLDLOCK)";
            if (Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) != 0) return null;
            var id = Guid.NewGuid();
            command.CommandText = "INSERT dbo.Accounts(UserId,FirstName,PaternalLastName,MaternalLastName,Email,NormalizedEmail,PasswordHash,DisplayName) VALUES(@id,@first,@last,@second,@email,@normalized,@hash,@display)";
            command.Id("@id", id); command.Text("@first", data.FirstName.Trim(),80); command.Text("@last",data.PaternalLastName.Trim(),80);
            command.Text("@second",data.MaternalLastName,80); command.Text("@email",data.Email.Trim(),254);
            command.Text("@normalized",data.Email.Trim().ToUpperInvariant(),254); command.Text("@hash",passwordHash,512);
            command.Text("@display",data.FirstName.Trim(),100);
            await command.ExecuteNonQueryAsync(cancellationToken); await transaction.CommitAsync(cancellationToken); return id;
        }
        /// <summary>Returns authentication material only within the server.</summary>
        /// <param name="email">The email used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<(Guid Id, string Hash)?> FindLoginAsync(string email, CancellationToken cancellationToken)
        {
            await using var connection = await connections.OpenAsync(cancellationToken); using var command = connection.CreateCommand();
            command.CommandText="SELECT UserId,PasswordHash FROM dbo.Accounts WHERE NormalizedEmail=@email AND IsEnabled=1";command.Text("@email",email.Trim().ToUpperInvariant(),254);
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);return await reader.ReadAsync(cancellationToken)?(reader.GetGuid(0),reader.GetString(1)):null;
        }
        /// <summary>
        /// Queries the authorized record or collection from persistent storage.
        /// </summary>
        /// <param name="userId">The user id used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<UserDetailsData?> GetAsync(Guid userId, CancellationToken cancellationToken)
        {
            await using var connection = await connections.OpenAsync(cancellationToken); using var command = connection.CreateCommand();
            command.CommandText="SELECT UserId,FirstName,PaternalLastName,MaternalLastName,Email,CreatedAt,IsOwner,DisplayName,Description,AvatarId,CoverImageId,PhoneNumber,AlternatePhoneNumber,DateOfBirth,Address,City,State,PostalCode,TaxIdentificationNumber,NationalIdentificationNumber FROM dbo.Accounts WHERE UserId=@id AND IsEnabled=1";command.Id("@id",userId);
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);if(!await reader.ReadAsync(cancellationToken))return null;
            return new UserDetailsData{UserId=reader.GetGuid(0),FirstName=reader.GetString(1),PaternalLastName=reader.GetString(2),MaternalLastName=reader.Optional(3),Email=reader.GetString(4),CreatedAt=reader.GetDateTimeOffset(5),IsOwner=reader.GetBoolean(6),Profile=new UserProfileData{DisplayName=reader.GetString(7),Description=reader.IsDBNull(8)?null:reader.GetString(8),AvatarId=reader.IsDBNull(9)?null:reader.GetGuid(9),CoverImageId=reader.IsDBNull(10)?null:reader.GetGuid(10),PhoneNumber=reader.IsDBNull(11)?null:reader.GetString(11),AlternatePhoneNumber=reader.IsDBNull(12)?null:reader.GetString(12),DateOfBirth=reader.IsDBNull(13)?null:DateOnly.FromDateTime(reader.GetDateTime(13)),Address=reader.IsDBNull(14)?null:reader.GetString(14),City=reader.IsDBNull(15)?null:reader.GetString(15),State=reader.IsDBNull(16)?null:reader.GetString(16),PostalCode=reader.IsDBNull(17)?null:reader.GetString(17),TaxIdentificationNumber=reader.IsDBNull(18)?null:reader.GetString(18),NationalIdentificationNumber=reader.IsDBNull(19)?null:reader.GetString(19)}};
        }
        /// <summary>
        /// Persists the complete private profile without changing authentication credentials.
        /// </summary>
        /// <param name="userId">The user id used by this operation.</param>
        /// <param name="data">The data used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        internal async Task UpdateProfileAsync(Guid userId, UserProfileData data, CancellationToken cancellationToken)
        {
            if(data.DateOfBirth>DateOnly.FromDateTime(DateTime.Today))throw new ArgumentException("La fecha de nacimiento no puede ser futura.");
            await using var connection=await connections.OpenAsync(cancellationToken); using var command=connection.CreateCommand();
            command.CommandText="UPDATE dbo.Accounts SET DisplayName=@DisplayName,Description=@Description,AvatarId=@AvatarId,CoverImageId=@CoverImageId,PhoneNumber=@PhoneNumber,AlternatePhoneNumber=@AlternatePhoneNumber,DateOfBirth=@DateOfBirth,Address=@Address,City=@City,State=@State,PostalCode=@PostalCode,TaxIdentificationNumber=@TaxIdentificationNumber,NationalIdentificationNumber=@NationalIdentificationNumber WHERE UserId=@id AND IsEnabled=1";
            command.Id("@id",userId);command.Text("@DisplayName",data.DisplayName,100);
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
            await ValidateMediaAsync(command,ProfileMedia(data.AvatarId,data.CoverImageId),userId,cancellationToken);
            if(await command.ExecuteNonQueryAsync(cancellationToken)!=1)throw new UnauthorizedAccessException();
        }
    #endregion Account Operations
}
