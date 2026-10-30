namespace CoffeManager.Client.Admin.Connection;
/// <summary>Composes entity clients for one authenticated connection without collecting their domain data.</summary>
/// <param name="connection">The connection used by this operation.</param>
internal sealed class AdminSession(ApiConnection connection)
{
    internal ApiConnection MediaConnection { get; }=connection;
    internal Models.User.Classes.User User { get; }=new(connection);
    internal Models.Business.Classes.Business Business { get; }=new(connection);
    internal Models.User.Classes.Employee Employee { get; }=new(connection);
    internal Models.Business.Classes.Branch Branch { get; }=new(connection);
}
