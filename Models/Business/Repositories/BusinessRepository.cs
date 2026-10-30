using CoffeManager.Infrastructure.Database;
using System.Data;
using static CoffeManager.Infrastructure.Database.MediaOwnership;
namespace CoffeManager.Models.Business.Repositories;
/// <summary>Persists the owner-scoped business identity and full profile.</summary>
/// <param name="connections">The connections used by this operation.</param>
internal sealed class BusinessRepository(SqlConnectionFactory connections)
{
    #region Business Operations
        /// <summary>
        /// Queries the authorized record or collection from persistent storage.
        /// </summary>
        /// <param name="ownerId">The owner id used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<BusinessDetailsData?> GetAsync(Guid ownerId,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="SELECT BusinessId,OwnerUserId,CreatedAt,BusinessType,TradeName,LegalName,Description,LogoId,CoverImageId,TaxIdentificationNumber,PhoneNumber,Email FROM dbo.Businesses WHERE OwnerUserId=@owner";command.Id("@owner",ownerId);
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);if(!await reader.ReadAsync(cancellationToken))return null;
            return new BusinessDetailsData{BusinessId=reader.GetGuid(0),OwnerUserId=reader.GetGuid(1),CreatedAt=reader.GetDateTimeOffset(2),BusinessType=reader.GetString(3),Profile=new BusinessProfileData{TradeName=reader.GetString(4),LegalName=reader.IsDBNull(5)?null:reader.GetString(5),Description=reader.IsDBNull(6)?null:reader.GetString(6),LogoId=reader.IsDBNull(7)?null:reader.GetGuid(7),CoverImageId=reader.IsDBNull(8)?null:reader.GetGuid(8),TaxIdentificationNumber=reader.IsDBNull(9)?null:reader.GetString(9),PhoneNumber=reader.IsDBNull(10)?null:reader.GetString(10),Email=reader.IsDBNull(11)?null:reader.GetString(11)}};
        }
        /// <summary>
        /// Creates the minimal record with a server-generated identifier.
        /// </summary>
        /// <param name="ownerId">The owner id used by this operation.</param>
        /// <param name="data">The data used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<Guid> CreateAsync(Guid ownerId,BusinessCreationData data,CancellationToken cancellationToken)
        {
            if(data.BusinessType!="Cafeteria")throw new ArgumentException("El giro disponible es Cafetería y repostería.");
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();var id=Guid.NewGuid();
            command.CommandText="INSERT dbo.Businesses(BusinessId,OwnerUserId,TradeName,BusinessType) VALUES(@id,@owner,@name,@type)";
            command.Id("@id",id);command.Id("@owner",ownerId);command.Text("@name",data.TradeName.Trim(),120);command.Text("@type",data.BusinessType,40);
            await command.ExecuteNonQueryAsync(cancellationToken);return id;
        }
        /// <summary>
        /// Persists the complete editable profile within the authorized owner scope.
        /// </summary>
        /// <param name="ownerId">The owner id used by this operation.</param>
        /// <param name="data">The data used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        internal async Task UpdateAsync(Guid ownerId,BusinessProfileData data,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="UPDATE dbo.Businesses SET TradeName=@TradeName,LegalName=@LegalName,Description=@Description,LogoId=@LogoId,CoverImageId=@CoverImageId,TaxIdentificationNumber=@TaxIdentificationNumber,PhoneNumber=@PhoneNumber,Email=@Email WHERE OwnerUserId=@owner";
            command.Id("@owner",ownerId);command.Text("@TradeName",data.TradeName,120);
            command.Text("@LegalName",data.LegalName,200);
            command.Text("@Description",data.Description,1000);
            command.Parameters.Add("@LogoId",SqlDbType.UniqueIdentifier).Value=(object?)data.LogoId??DBNull.Value;
            command.Parameters.Add("@CoverImageId",SqlDbType.UniqueIdentifier).Value=(object?)data.CoverImageId??DBNull.Value;
            command.Text("@TaxIdentificationNumber",data.TaxIdentificationNumber,13);
            command.Text("@PhoneNumber",data.PhoneNumber,30);
            command.Text("@Email",data.Email,254);
            await ValidateMediaAsync(command,ProfileMedia(data.LogoId,data.CoverImageId),ownerId,cancellationToken);
            if(await command.ExecuteNonQueryAsync(cancellationToken)!=1)throw new KeyNotFoundException();
        }
    #endregion Business Operations
}
