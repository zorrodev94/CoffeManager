using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using CoffeManager.Infrastructure.Database;
using CoffeManager.Infrastructure.Api;
using CoffeManager.Models.User.Classes;
using CoffeManager.Models.User.Repositories;
using CoffeManager.Models.Business.Classes;
using CoffeManager.Models.Business.Repositories;


var builder=WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options=>options.Limits.MaxRequestBodySize=3*1024*1024);
builder.Services.AddSingleton<SqlConnectionFactory>();
builder.Services.AddScoped<CoffeManager.Models.Media.Classes.Media>();
builder.Services.AddScoped<AccountRepository>();builder.Services.AddScoped<Account>();builder.Services.AddScoped<IPasswordHasher<CoffeManager.Models.User.Base.User>,PasswordHasher<CoffeManager.Models.User.Base.User>>();
builder.Services.AddScoped<BusinessRepository>();builder.Services.AddScoped<Business>();
builder.Services.AddScoped<EmployeeRepository>();builder.Services.AddScoped<Employee>();
builder.Services.AddScoped<BranchRepository>();builder.Services.AddScoped<CoffeManager.Models.Business.Classes.Branch>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options=>
{
    options.Cookie.Name="CoffeManager.Admin";options.Cookie.HttpOnly=true;options.Cookie.SecurePolicy=CookieSecurePolicy.Always;options.Cookie.SameSite=SameSiteMode.Strict;
    options.ExpireTimeSpan=TimeSpan.FromHours(8);options.SlidingExpiration=false;
    options.Events.OnRedirectToLogin=context=>{context.Response.StatusCode=401;return Task.CompletedTask;};
    options.Events.OnRedirectToAccessDenied=context=>{context.Response.StatusCode=403;return Task.CompletedTask;};
    options.Events.OnValidatePrincipal=async context=>
    {
        var id=context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if(!Guid.TryParse(id,out var userId)){context.RejectPrincipal();return;}
        var access=await context.HttpContext.RequestServices.GetRequiredService<AccountRepository>().GetAccessAsync(userId,context.HttpContext.RequestAborted);
        var version=context.Principal?.FindFirst("AuthVersion")?.Value;
        if(access is null||!access.Value.CanAdmin||version!=access.Value.Version.ToString())context.RejectPrincipal();
    };
});
builder.Services.AddAuthorization(options=>{options.AddPolicy("Owner",policy=>policy.RequireAuthenticatedUser().RequireRole("Owner"));options.AddPolicy("Administration",policy=>policy.RequireAuthenticatedUser().RequireRole("Owner","Admin"));});
builder.Services.AddRateLimiter(options=>
{
    options.RejectionStatusCode=429;
    options.AddPolicy("login",context=>RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString()??"local",_=>new FixedWindowRateLimiterOptions{PermitLimit=5,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));
});
var app=builder.Build();
if(args.Contains("--initialize-database"))
{
    await DatabaseInitializer.InitializeAsync(app.Services.GetRequiredService<SqlConnectionFactory>(),app.Environment.ContentRootPath,CancellationToken.None);await DatabaseVerifier.VerifyAsync(app.Services.GetRequiredService<SqlConnectionFactory>(),CancellationToken.None);return;
}
if(args.Contains("--verify-database")){await DatabaseVerifier.VerifyAsync(app.Services.GetRequiredService<SqlConnectionFactory>(),CancellationToken.None);return;}
app.Use(async(context,next)=>
{
    try
    {
        if(context.Request.Path.StartsWithSegments("/api")&&context.Request.Method!="GET"&&context.Request.Headers["X-CoffeManager-Client"]!="Admin")
        {context.Response.StatusCode=400;await context.Response.WriteAsJsonAsync(new{detail="Falta la cabecera del cliente autorizado."});return;}
        await next(context);
    }
    catch(Exception exception) when(exception is SqlException or KeyNotFoundException or ArgumentException or UnauthorizedAccessException)
    {
        var status=exception switch {SqlException sql when sql.Number is 2601 or 2627=>409,SqlException sql when sql.Number==547=>400,SqlException=>503,KeyNotFoundException=>404,UnauthorizedAccessException=>403,_=>400};
        if(exception is SqlException)app.Logger.LogWarning("Database operation failed; code {Code}",((SqlException)exception).Number);
        context.Response.StatusCode=status;await context.Response.WriteAsJsonAsync(new{detail=status switch{503=>"No fue posible acceder a la base de datos. Revisa conexión y esquema.",409=>"El registro ya existe.",404=>"Registro no encontrado.",403=>"Acceso no permitido.",_=>exception is ArgumentException?exception.Message:"Revisa los datos de la solicitud."}});
    }
});
app.UseHttpsRedirection();app.UseRateLimiter();app.UseAuthentication();app.UseAuthorization();app.MapCoffeManager();app.Run();
