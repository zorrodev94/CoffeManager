using CoffeManager.Client.Admin.Connection;
namespace CoffeManager.Client.Admin.Models.User.Classes;
/// <summary>Provides the concrete account entity used by this application.</summary>
internal sealed class User(ApiConnection connection) : Base.User(connection);
