using CoffeManager.Client.Admin.Connection;
using System.ComponentModel.DataAnnotations;
namespace CoffeManager.Client.Admin.Models.User.Classes;
/// <summary>Provides the concrete employee entity for this application.</summary>
internal sealed class Employee(ApiConnection connection):EmployeeUser(connection);
