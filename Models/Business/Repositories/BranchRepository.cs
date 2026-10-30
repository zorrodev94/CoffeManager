using CoffeManager.Infrastructure.Database;
using System.Data;
using static CoffeManager.Infrastructure.Database.MediaOwnership;
namespace CoffeManager.Models.Business.Repositories;
/// <summary>Persists branches within their authorized business scope.</summary>
/// <param name="connections">The connections used by this operation.</param>
internal sealed class BranchRepository(SqlConnectionFactory connections)
{
    #region Branch Operations
        /// <summary>
        /// Queries the authorized record or collection from persistent storage.
        /// </summary>
        /// <param name="ownerId">The owner id used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<IReadOnlyList<BranchDetailsData>> GetAsync(Guid ownerId,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="SELECT s.BranchId,s.BusinessId,s.CreatedAt,s.Name,s.Description,s.Address,s.Neighborhood,s.City,s.State,s.PostalCode,s.PhoneNumber,s.Email,s.LogoId,s.CoverImageId FROM dbo.Branches s INNER JOIN dbo.Businesses b ON b.BusinessId=s.BusinessId WHERE b.OwnerUserId=@owner ORDER BY s.CreatedAt,s.BranchId";command.Id("@owner",ownerId);
            await using var reader=await command.ExecuteReaderAsync(cancellationToken);var items=new List<BranchDetailsData>();
            while(await reader.ReadAsync(cancellationToken))items.Add(new BranchDetailsData{BranchId=reader.GetGuid(0),BusinessId=reader.GetGuid(1),CreatedAt=reader.GetDateTimeOffset(2),Name=reader.GetString(3),Profile=new BranchProfileData{Name=reader.GetString(3),Description=reader.IsDBNull(4)?null:reader.GetString(4),Address=reader.IsDBNull(5)?null:reader.GetString(5),Neighborhood=reader.IsDBNull(6)?null:reader.GetString(6),City=reader.IsDBNull(7)?null:reader.GetString(7),State=reader.IsDBNull(8)?null:reader.GetString(8),PostalCode=reader.IsDBNull(9)?null:reader.GetString(9),PhoneNumber=reader.IsDBNull(10)?null:reader.GetString(10),Email=reader.IsDBNull(11)?null:reader.GetString(11),LogoId=reader.IsDBNull(12)?null:reader.GetGuid(12),CoverImageId=reader.IsDBNull(13)?null:reader.GetGuid(13)}});return items;
        }
        /// <summary>
        /// Creates the minimal record with a server-generated identifier.
        /// </summary>
        /// <param name="ownerId">The owner id used by this operation.</param>
        /// <param name="data">The data used by this operation.</param>
        /// <param name="cancellationToken">The cancellation token used by this operation.</param>
        /// <returns>The result of the requested operation.</returns>
        internal async Task<Guid?> CreateAsync(Guid ownerId,BranchCreationData data,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();var id=Guid.NewGuid();
            command.CommandText="INSERT dbo.Branches(BranchId,BusinessId,Name) SELECT @id,BusinessId,@name FROM dbo.Businesses WHERE BusinessId=@business AND OwnerUserId=@owner";
            command.Id("@id",id);command.Id("@business",data.BusinessId);command.Id("@owner",ownerId);command.Text("@name",data.Name.Trim(),120);
            return await command.ExecuteNonQueryAsync(cancellationToken)==1?id:null;
        }
        /// <summary>Updates a branch only when its business belongs to the authenticated owner.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="branchId">The selected branch identifier.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task UpdateAsync(Guid ownerId,Guid branchId,BranchProfileData data,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();
            command.CommandText="UPDATE s SET Name=@Name,Description=@Description,Address=@Address,Neighborhood=@Neighborhood,City=@City,State=@State,PostalCode=@PostalCode,PhoneNumber=@PhoneNumber,Email=@Email,LogoId=@LogoId,CoverImageId=@CoverImageId FROM dbo.Branches s INNER JOIN dbo.Businesses b ON b.BusinessId=s.BusinessId WHERE s.BranchId=@id AND b.OwnerUserId=@owner";
            command.Id("@id",branchId);command.Id("@owner",ownerId);command.Text("@Name",data.Name,120);
            command.Text("@Description",data.Description,1000);
            command.Text("@Address",data.Address,250);
            command.Text("@Neighborhood",data.Neighborhood,100);
            command.Text("@City",data.City,100);
            command.Text("@State",data.State,100);
            command.Text("@PostalCode",data.PostalCode,10);
            command.Text("@PhoneNumber",data.PhoneNumber,30);
            command.Text("@Email",data.Email,254);
            command.Parameters.Add("@LogoId",SqlDbType.UniqueIdentifier).Value=(object?)data.LogoId??DBNull.Value;
            command.Parameters.Add("@CoverImageId",SqlDbType.UniqueIdentifier).Value=(object?)data.CoverImageId??DBNull.Value;
            await ValidateMediaAsync(command,ProfileMedia(data.LogoId,data.CoverImageId),ownerId,cancellationToken);
            if(await command.ExecuteNonQueryAsync(cancellationToken)!=1)throw new KeyNotFoundException();
        }
    #endregion Branch Operations
}
