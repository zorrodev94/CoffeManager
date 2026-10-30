using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using CoffeManager.Models.User.Classes;
using CoffeManager.Models.User.Repositories;
using CoffeManager.Models.Business.Classes;


namespace CoffeManager.Infrastructure.Api;
internal static class ApiEndpoints
{
    /// <summary>
    /// Maps validated authentication and owner-scoped administration endpoints.
    /// </summary>
    /// <param name="app">The app used by this operation.</param>
    internal static void MapCoffeManager(this WebApplication app)
    {
        var api=app.MapGroup("/api");
        api.MapGet("/health",()=>Results.Ok(new {status="API disponible"}));
        api.MapPost("/accounts/bootstrap",async (AccountRegistrationData data,Account account,IConfiguration configuration,CancellationToken token)=>
        {
            var key=configuration["Setup:Key"];
            if(string.IsNullOrWhiteSpace(key)||key.Length<24)return Results.Problem("Configura Setup:Key para activar el primer registro.",statusCode:503);
            if(!CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(key)),SHA256.HashData(Encoding.UTF8.GetBytes(data.SetupKey))))return Results.Unauthorized();
            var id=await account.RegisterInitialAsync(data,token);
            return id is null?Results.Conflict(new{detail="La cuenta inicial ya existe."}):Results.Ok(id.Value);
        }).AddEndpointFilter<RequestValidation<AccountRegistrationData>>().RequireRateLimiting("login");
        api.MapPost("/accounts/login",async(LoginData data,Account account,AccountRepository repository,HttpContext context,CancellationToken token)=>
        {
            var id=await account.AuthenticateAsync(data,token);if(id is null)return Results.Unauthorized();
            var access=await repository.GetAccessAsync(id.Value,token);if(access is null||!access.Value.CanAdmin)return Results.Unauthorized();
            var identity=new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,id.Value.ToString()),new Claim("OwnerId",access.Value.OwnerId.ToString()),new Claim("AuthVersion",access.Value.Version.ToString()),new Claim(ClaimTypes.Role,access.Value.IsOwner?"Owner":"Admin")],CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(identity),new AuthenticationProperties{IsPersistent=false});return Results.NoContent();
        }).AddEndpointFilter<RequestValidation<LoginData>>().RequireRateLimiting("login");
        var secured=api.MapGroup("").RequireAuthorization("Administration");
        secured.MapPost("/accounts/logout",async(HttpContext context)=>{await context.SignOutAsync();return Results.NoContent();});
        secured.MapGet("/users/me",async(HttpContext context,Account account,CancellationToken token)=>{var result=await account.GetAsync(UserId(context),token);return result is null?Results.NotFound():Results.Ok(result.Value);});
        secured.MapPut("/users/me",async(UserProfileData data,HttpContext context,Account account,CancellationToken token)=>{await account.UpdateAsync(UserId(context),data,token);return Results.NoContent();}).AddEndpointFilter<RequestValidation<UserProfileData>>();
        secured.MapGet("/business",async(HttpContext context,Business business,CancellationToken token)=>
        {
            var result=await business.GetAsync(Owner(context),token);
            return result is null?Results.NoContent():Results.Ok(result.Value);
        });
        secured.MapPost("/business",async(BusinessCreationData data,HttpContext context,Business business,CancellationToken token)=>Results.Ok(await business.RegisterAsync(Owner(context),data,token))).AddEndpointFilter<RequestValidation<BusinessCreationData>>();
        secured.MapPut("/business",async(BusinessProfileData data,HttpContext context,Business business,CancellationToken token)=>{await business.UpdateAsync(Owner(context),data,token);return Results.NoContent();}).AddEndpointFilter<RequestValidation<BusinessProfileData>>();
        secured.MapGet("/employees",async(HttpContext context,Employee employee,CancellationToken token)=>Results.Ok(await employee.GetEmployeesAsync(Owner(context),token)));
        secured.MapPost("/employees",async(EmployeeCreationData data,HttpContext context,Employee employee,CancellationToken token)=>Results.Ok(await employee.RegisterEmployeeAsync(Owner(context),data,token))).AddEndpointFilter<RequestValidation<EmployeeCreationData>>();
        secured.MapPut("/employees/{id:guid}/profile",async(Guid id,EmployeeProfileData data,HttpContext context,Employee employee,CancellationToken token)=>{await employee.UpdateEmployeeAsync(Owner(context),id,data,token);return Results.NoContent();}).AddEndpointFilter<RequestValidation<EmployeeProfileData>>();
        secured.MapPut("/employees/{id:guid}/assignment",async(Guid id,EmployeeAssignmentData data,HttpContext context,Employee employee,CancellationToken token)=>{await employee.AssignEmployeeAsync(Owner(context),id,data,token);return Results.NoContent();}).AddEndpointFilter<RequestValidation<EmployeeAssignmentData>>().RequireAuthorization("Owner");
        secured.MapGet("/roles",async(Account account,CancellationToken token)=>Results.Ok(await account.GetRolesAsync(token)));
        secured.MapPost("/roles",async(RoleCreationData data,Account account,CancellationToken token)=>Results.Ok(await account.AddRoleAsync(data,token))).AddEndpointFilter<RequestValidation<RoleCreationData>>().RequireAuthorization("Owner");
        secured.MapPost("/accounts/staff",async(StaffAccountRegistrationData data,Account account,HttpContext context,CancellationToken token)=>Results.Ok(await account.RegisterStaffAsync(Owner(context),data,token))).AddEndpointFilter<RequestValidation<StaffAccountRegistrationData>>().RequireAuthorization("Owner");
        secured.MapGet("/accounts/available",async(Account account,HttpContext context,CancellationToken token)=>Results.Ok(await account.GetAvailableAccountsAsync(Owner(context),token))).RequireAuthorization("Owner");
        secured.MapGet("/catalogs/{catalog}",async(string catalog,Employee employee,CancellationToken token)=>Results.Ok(await employee.GetCatalogAsync(catalog,token)));
        secured.MapPost("/catalogs/{catalog}",async(string catalog,CatalogCreationData data,Employee employee,CancellationToken token)=>Results.Ok(await employee.AddCatalogOptionAsync(catalog,data,token))).AddEndpointFilter<RequestValidation<CatalogCreationData>>().RequireAuthorization("Owner");
        secured.MapPut("/branches/{id:guid}/profile",async(Guid id,BranchProfileData data,HttpContext context,CoffeManager.Models.Business.Classes.Branch branch,CancellationToken token)=>{await branch.UpdateAsync(Owner(context),id,data,token);return Results.NoContent();}).AddEndpointFilter<RequestValidation<BranchProfileData>>();
        secured.MapPost("/media",async(HttpContext context,CoffeManager.Models.Media.Classes.Media media,CancellationToken token)=>
        {
            if(context.Request.ContentLength>CoffeManager.Models.Media.Classes.Media.MaxImageBytes)return Results.Problem("La imagen supera el límite de 2 MB.",statusCode:413);
            using var buffer=new MemoryStream();var chunk=new byte[8192];int count;
            while((count=await context.Request.Body.ReadAsync(chunk,token))>0){if(buffer.Length+count>CoffeManager.Models.Media.Classes.Media.MaxImageBytes)return Results.Problem("La imagen supera el límite de 2 MB.",statusCode:413);await buffer.WriteAsync(chunk.AsMemory(0,count),token);}
            return Results.Ok(await media.UploadAsync(Owner(context),buffer.ToArray(),token));
        });
        secured.MapGet("/media/{id:guid}",async(Guid id,HttpContext context,CoffeManager.Models.Media.Classes.Media media,CancellationToken token)=>
        {
            var item=await media.GetAsync(Owner(context),id,token);context.Response.Headers["Cache-Control"]="private, no-store";context.Response.Headers["X-Content-Type-Options"]="nosniff";return item is null?Results.NotFound():Results.File(item.Value.Path,item.Value.Type);
        });
        secured.MapGet("/branches",async(HttpContext context,CoffeManager.Models.Business.Classes.Branch branch,CancellationToken token)=>Results.Ok(await branch.GetAsync(Owner(context),token)));
        secured.MapPost("/branches",async(BranchCreationData data,HttpContext context,CoffeManager.Models.Business.Classes.Branch branch,IConfiguration configuration,IWebHostEnvironment environment,CancellationToken token)=>
        {
            if(!environment.IsDevelopment()||!configuration.GetValue<bool>("DevelopmentFeatures:AllowUnlicensedBranchCreation"))return Results.Problem("La creación de sucursales permanece deshabilitada hasta integrar los límites de suscripción. Para una prueba local puede habilitarse explícitamente.",statusCode:409);
            var id=await branch.RegisterAsync(Owner(context),data,token);return id is null?Results.NotFound():Results.Ok(id.Value);
        }).AddEndpointFilter<RequestValidation<BranchCreationData>>();
    }
    /// <summary>
    /// Resolves the owner scope from the authenticated session.
    /// </summary>
    /// <param name="context">The context used by this operation.</param>
    /// <returns>The result of the requested operation.</returns>
    private static Guid Owner(HttpContext context)=>Guid.Parse(context.User.FindFirstValue("OwnerId")!);
    private static Guid UserId(HttpContext context)=>Guid.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
