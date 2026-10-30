using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CoffeManager.Infrastructure.Database;
using CoffeManager.Models.User.Repositories;
using CoffeManager.Models.Business.Repositories;
using CoffeManager.Models.User.Classes;
using UserBase=CoffeManager.Models.User.Base.User;
using MediaEntity=CoffeManager.Models.Media.Classes.Media;
using static CoffeManager.Models.User.Base.User;
using static CoffeManager.Models.User.Classes.EmployeeUser;
using static CoffeManager.Models.Business.Classes.Business;
using static CoffeManager.Models.Business.Classes.Branch;
var profiles=new[]{typeof(UserProfileData),typeof(EmployeeProfileData),typeof(BusinessProfileData),typeof(BranchProfileData)};
foreach(var type in profiles)
{
    var sample=Sample(type);var serialized=JsonSerializer.Serialize(sample,type);var restored=JsonSerializer.Deserialize(serialized,type)!;Equal(sample,restored,type);
    foreach(var property in type.GetProperties().Where(x=>x.PropertyType==typeof(string)))
    {
        var bound=property.GetCustomAttribute<StringLengthAttribute>();if(bound is null)throw new Exception("Unbounded field: "+type.Name+"."+property.Name);
        var invalid=Sample(type);property.SetValue(invalid,new string('x',bound.MaximumLength+1));var results=new List<ValidationResult>();Assert(!Validator.TryValidateObject(invalid,new ValidationContext(invalid),results,true),"Oversized field accepted: "+property.Name);
    }
    Console.WriteLine("PASS complete profile serialization and field limits: "+type.Name);
}
var png=Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");Assert(MediaEntity.InspectImage(png)=="image/png","PNG rejected");
foreach(var invalid in new[]{new byte[24],new byte[MediaEntity.MaxImageBytes+1]}){try{MediaEntity.InspectImage(invalid);throw new Exception("Invalid image accepted");}catch(ArgumentException){}}
Console.WriteLine("PASS image format and size limits");
var source=Environment.GetEnvironmentVariable("COFFEMANAGER_TEST_SQL");
if(string.IsNullOrWhiteSpace(source)){Console.WriteLine("SKIP live SQL: set COFFEMANAGER_TEST_SQL to an instance connection string with CREATE DATABASE permission. No SQL integration was run.");return;}
var root=Path.GetFullPath(args.FirstOrDefault()??".");Assert(File.Exists(Path.Combine(root,"CoffeManager.csproj")),"Run this test from the solution directory.");
var db="CoffeManager_Audit_"+Guid.NewGuid().ToString("N");var builder=new SqlConnectionStringBuilder(source){InitialCatalog="master",Pooling=false};var created=false;
await using var master=new SqlConnection(builder.ConnectionString);await master.OpenAsync();var mediaRoot=Path.Combine(Path.GetTempPath(),db);
try
{
    using(var create=master.CreateCommand()){create.CommandText=$"CREATE DATABASE [{db}]";await create.ExecuteNonQueryAsync();created=true;}
    builder.InitialCatalog=db;var config=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"ConnectionStrings:CoffeManager",builder.ConnectionString},{"Media:RootPath",mediaRoot}}).Build();var connections=new SqlConnectionFactory(config);var token=CancellationToken.None;
    // Reproduce an existing v1 database before applying the additive migration.
    await using(var old=await connections.OpenAsync(token)){using var script=old.CreateCommand();script.CommandText=await File.ReadAllTextAsync(Path.Combine(root,"Infrastructure/Database/Scripts/001_initial.sql"));await script.ExecuteNonQueryAsync();}
    var accounts=new AccountRepository(connections);var hasher=new PasswordHasher<UserBase>();var account=new Account(accounts,hasher);
    var initial=new AccountRegistrationData{FirstName="Audit",PaternalLastName="Owner",Email="audit-owner@example.invalid",Password="Audit-Only-Password-2026",SetupKey=new string('x',32)};
    var owner=(await account.RegisterInitialAsync(initial,token))!.Value;var hashBefore=(await accounts.FindLoginAsync(initial.Email,token))!.Value.Hash;
    await DatabaseInitializer.InitializeAsync(connections,root,token);await DatabaseInitializer.InitializeAsync(connections,root,token);await DatabaseVerifier.VerifyAsync(connections,token);
    Assert((await accounts.FindLoginAsync(initial.Email,token))!.Value.Hash==hashBefore,"Migration changed owner credentials");Assert(await account.RegisterInitialAsync(initial,token) is null,"Duplicate owner accepted");
    var environment=WebApplication.CreateBuilder(new WebApplicationOptions{ContentRootPath=root}).Environment;var media=new MediaEntity(connections,config,environment);var image=await media.UploadAsync(owner,png,token);
    var userProfile=(UserProfileData)Sample(typeof(UserProfileData));userProfile.AvatarId=image;userProfile.CoverImageId=image;await account.UpdateAsync(owner,userProfile,token);Equal(userProfile,(await account.GetAsync(owner,token))!.Value.Profile,typeof(UserProfileData));
    var businesses=new BusinessRepository(connections);var business=await businesses.CreateAsync(owner,new(){TradeName="Audit Café",BusinessType="Cafeteria"},token);var businessProfile=(BusinessProfileData)Sample(typeof(BusinessProfileData));businessProfile.LogoId=image;businessProfile.CoverImageId=image;await businesses.UpdateAsync(owner,businessProfile,token);Equal(businessProfile,(await businesses.GetAsync(owner,token))!.Value.Profile,typeof(BusinessProfileData));
    var branches=new BranchRepository(connections);var branch=(await branches.CreateAsync(owner,new(){BusinessId=business,Name="Audit Branch"},token))!.Value;var branchProfile=(BranchProfileData)Sample(typeof(BranchProfileData));branchProfile.LogoId=image;branchProfile.CoverImageId=image;await branches.UpdateAsync(owner,branch,branchProfile,token);Equal(branchProfile,(await branches.GetAsync(owner,token)).Single().Profile,typeof(BranchProfileData));
    var employees=new EmployeeRepository(connections);var employee=await employees.CreateAsync(owner,new(){FirstName="Audit",PaternalLastName="Employee"},token);var employeeProfile=(EmployeeProfileData)Sample(typeof(EmployeeProfileData));employeeProfile.AvatarId=image;employeeProfile.CoverImageId=image;employeeProfile.EmploymentStatusId=(await employees.GetCatalogAsync("employment-statuses",token)).Single(x=>x.Code=="Active").Id;employeeProfile.SalaryBasisId=(await employees.GetCatalogAsync("salary-bases",token)).Single(x=>x.Code=="Monthly").Id;await employees.UpdateAsync(owner,employee,employeeProfile,token);Equal(employeeProfile,(await employees.GetAsync(owner,token)).Single().Profile,typeof(EmployeeProfileData));
    var staff=await account.RegisterStaffAsync(owner,new(){FirstName="Access",PaternalLastName="Audit",Email="audit-staff@example.invalid",Password="Audit-Only-Password-2026"},token);Assert(!(await accounts.GetAccessAsync(staff,token))!.Value.CanAdmin,"Unlinked account has access");
    var roles=await accounts.GetRolesAsync(token);var admin=roles.Single(x=>x.Name=="Administrador").RoleId;var noAccess=roles.Single(x=>x.Name=="Empleado").RoleId;
    try{await employees.AssignAsync(owner,employee,new(){BranchId=branch,RoleId=admin},token);throw new Exception("Role without account accepted");}catch(ArgumentException){}
    await employees.AssignAsync(owner,employee,new(){BranchId=branch,RoleId=admin,UserAccountId=staff},token);var before=(await accounts.GetAccessAsync(staff,token))!.Value;Assert(before.CanAdmin,"Assigned administrator has no access");Assert(!(await accounts.GetAvailableAccountsAsync(owner,token)).Any(x=>x.UserAccountId==staff||x.UserAccountId==owner),"Linked or owner account is eligible");
    var staffHash=(await accounts.FindLoginAsync("audit-staff@example.invalid",token))!.Value.Hash;
    await employees.AssignAsync(owner,employee,new(){BranchId=branch,RoleId=noAccess,UserAccountId=staff},token);var after=(await accounts.GetAccessAsync(staff,token))!.Value;Assert(!after.CanAdmin&&!after.CanCheckout&&after.Version>before.Version,"Demotion did not revoke access");Assert((await accounts.FindLoginAsync("audit-staff@example.invalid",token))!.Value.Hash==staffHash,"Demotion changed password");
    var second=await employees.CreateAsync(owner,new(){FirstName="Second",PaternalLastName="Audit"},token);try{await employees.AssignAsync(owner,second,new(){BranchId=branch,RoleId=admin,UserAccountId=staff},token);throw new Exception("Duplicate account assignment accepted");}catch(ArgumentException){}
    Assert((await employees.GetAsync(Guid.NewGuid(),token)).Count==0,"Cross-owner employee query leaked data");Assert(await branches.CreateAsync(Guid.NewGuid(),new(){BusinessId=business,Name="Forbidden"},token) is null,"Cross-owner branch accepted");
    Console.WriteLine("PASS LIVE SQL: v1 migration, idempotent initialization, complete profile round trips, image ownership, account eligibility, role requirements and demotion with preserved password.");
}
finally
{
    if(created){using var drop=master.CreateCommand();drop.CommandText=$"ALTER DATABASE [{db}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{db}];";await drop.ExecuteNonQueryAsync();}
    if(Directory.Exists(mediaRoot))Directory.Delete(mediaRoot,true);
}
static object Sample(Type type)
{
    var value=Activator.CreateInstance(type)!;foreach(var property in type.GetProperties())
    {
        var t=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;
        object? sample=t==typeof(string)?property.Name=="Email"?"audit@example.invalid":property.Name=="TaxIdentificationNumber"?"ABC010101ABC":property.Name=="NationalIdentificationNumber"?"AAAA010101HSLAAA01":"Audit":t==typeof(Guid)?Guid.NewGuid():t==typeof(DateOnly)?new DateOnly(2000,1,2):t==typeof(decimal)?1234.56m:null;property.SetValue(value,sample);
    }return value;
}
static void Equal(object expected,object actual,Type type){foreach(var property in type.GetProperties())Assert(Equals(property.GetValue(expected),property.GetValue(actual)),"Field round trip mismatch: "+type.Name+"."+property.Name);}
static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
