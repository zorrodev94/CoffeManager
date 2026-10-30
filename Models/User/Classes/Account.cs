using Microsoft.AspNetCore.Identity;
using CoffeManager.Models.User.Repositories;
namespace CoffeManager.Models.User.Classes;
/// <summary>Provides the concrete account entity used by this application.</summary>
internal sealed class Account(AccountRepository repository,IPasswordHasher<Base.User> hasher) : Base.User(repository,hasher);
