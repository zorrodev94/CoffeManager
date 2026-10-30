using System.ComponentModel.DataAnnotations;
namespace CoffeManager.Infrastructure.Api;
internal sealed class RequestValidation<T> : IEndpointFilter where T : struct
{
    /// <summary>
    /// Invoke for the current entity operation.
    /// </summary>
    /// <param name="context">The context used by this operation.</param>
    /// <param name="next">The next used by this operation.</param>
    /// <returns>The result of the requested operation.</returns>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context,EndpointFilterDelegate next)
    {
        object value=context.Arguments.OfType<T>().First();var failures=new List<ValidationResult>();
        if(!Validator.TryValidateObject(value,new ValidationContext(value),failures,true))
            return Results.ValidationProblem(failures.SelectMany(f=>f.MemberNames.DefaultIfEmpty("request").Select(n=>(Name:n,Message:f.ErrorMessage??"Dato inválido"))).GroupBy(x=>x.Name).ToDictionary(g=>g.Key,g=>g.Select(x=>x.Message).ToArray()));
        return await next(context);
    }
}
