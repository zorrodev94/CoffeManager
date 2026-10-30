using Microsoft.Data.SqlClient;
namespace CoffeManager.Infrastructure.Database;
/// <summary>Validates that profile media belong to the authenticated business owner.</summary>
internal static class MediaOwnership
{
    #region Validation Operations
        internal static Guid?[] ProfileMedia(Guid? first,Guid? second)=>[first,second];
        /// <summary>Checks profile references before updating them; foreign keys preserve referential integrity.</summary>
        internal static async Task ValidateMediaAsync(SqlCommand update,Guid?[] mediaIds,Guid userId,CancellationToken cancellationToken)
        {
            foreach(var id in mediaIds.Where(x=>x.HasValue).Distinct())
            {
                using var command=update.Connection!.CreateCommand();command.Transaction=update.Transaction;
                command.CommandText="SELECT COUNT(*) FROM dbo.MediaFiles m INNER JOIN dbo.Accounts a ON m.OwnerUserId=COALESCE(a.OwnerUserId,a.UserId) WHERE a.UserId=@user AND m.MediaId=@media";
                command.Id("@user",userId);command.Id("@media",id!.Value);
                if(Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken))!=1)throw new ArgumentException("La imagen seleccionada no pertenece a este negocio.");
            }
        }
    #endregion Validation Operations
}
