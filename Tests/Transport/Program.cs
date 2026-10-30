using System.Net;
using CoffeManager.Client.Admin.Connection;
foreach(var test in new[]{(HttpStatusCode.NoContent,""),(HttpStatusCode.OK,""),(HttpStatusCode.OK,"null")})
{
    using var connection=new ApiConnection(new Uri("https://localhost/"),new Reply(test.Item1,test.Item2));
    if(await connection.GetAsync<string>("api/business") is not null)throw new Exception("Expected an empty business result.");
}
using(var connection=new ApiConnection(new Uri("https://localhost/"),new Reply(HttpStatusCode.OK,"{\"name\":\"Café\"}")))
{
    var result=await connection.GetAsync<Dictionary<string,string>>("api/business");
    if(result?["name"]!="Café")throw new Exception("Valid JSON was not preserved.");
}
using(var connection=new ApiConnection(new Uri("https://localhost/"),new Reply(HttpStatusCode.OK,"<html>proxy error</html>")))
{
    try{await connection.GetAsync<string>("api/business");throw new Exception("Invalid JSON was accepted.");}
    catch(InvalidOperationException error) when(error.InnerException is System.Text.Json.JsonException) { }
}
using(var connection=new ApiConnection(new Uri("https://localhost/"),new Reply(HttpStatusCode.OK,"<html>bad gateway</html>")))
{
    try{await connection.CreateAsync("api/business",new CreateRequest{Name="Valid"});throw new Exception("Malformed creation response accepted");}
    catch(InvalidOperationException error) when(error.InnerException is System.Text.Json.JsonException) { }
}
Console.WriteLine("PASS: 204, empty 200, null JSON, valid JSON, malformed query and malformed creation response.");
sealed class Reply(HttpStatusCode status,string body):HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>Task.FromResult(new HttpResponseMessage(status){Content=new StringContent(body,System.Text.Encoding.UTF8,"application/json")});
}

internal struct CreateRequest{public string Name{get;set;}}
