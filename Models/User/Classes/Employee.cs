using Microsoft.AspNetCore.Identity;
using CoffeManager.Models.User.Repositories;
using System.ComponentModel.DataAnnotations;
namespace CoffeManager.Models.User.Classes;
/// <summary>Provides the concrete employee entity for this application.</summary>
internal sealed class Employee(EmployeeRepository employees,AccountRepository accounts,IPasswordHasher<Base.User> hasher):EmployeeUser(employees,accounts,hasher);
