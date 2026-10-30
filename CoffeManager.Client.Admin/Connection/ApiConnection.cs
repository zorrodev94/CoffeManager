using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
namespace CoffeManager.Client.Admin.Connection;
internal sealed class ApiConnection : IDisposable
{
    #region Operation Data
        private readonly HttpClient _http;
    #endregion Operation Data
    #region Initialization
        internal ApiConnection(Uri address) : this(address,new HttpClientHandler{CookieContainer=new CookieContainer(),UseCookies=true}) { }
        /// <summary>Creates a connection with an explicit transport for isolated response tests.</summary>
        internal ApiConnection(Uri address,HttpMessageHandler transport)
        {
            if(!address.IsAbsoluteUri||address.Scheme!=Uri.UriSchemeHttps)throw new ArgumentException("Configura una API HTTPS válida.");
            _http=new HttpClient(transport){BaseAddress=address,Timeout=TimeSpan.FromSeconds(20),MaxResponseContentBufferSize=4*1024*1024};
            _http.DefaultRequestHeaders.Add("X-CoffeManager-Client","Admin");
        }
    #endregion Initialization
    #region Request Operations
        internal async Task<T?> GetAsync<T>(string route,CancellationToken token=default)
        {
            using var response=await _http.GetAsync(route,token);await VerifyAsync(response,token);
            if(response.StatusCode==HttpStatusCode.NoContent)return default;
            var body=await response.Content.ReadAsStringAsync(token);
            if(string.IsNullOrWhiteSpace(body))return default;
            try{return JsonSerializer.Deserialize<T>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web));}
            catch(JsonException error){throw new InvalidOperationException("La API devolvió una respuesta inesperada. Revisa la versión del servidor y del cliente.",error);}
        }
        internal async Task<Guid> CreateAsync<T>(string route,T data,CancellationToken token=default) where T:struct
        {
            Validate(data);using var response=await _http.PostAsJsonAsync(route,data,token);await VerifyAsync(response,token);
            return await ReadIdentifierAsync(response,token);
        }
        internal async Task SendAsync<T>(HttpMethod method,string route,T data,CancellationToken token=default) where T:struct
        {
            Validate(data);using var request=new HttpRequestMessage(method,route){Content=JsonContent.Create(data)};
            using var response=await _http.SendAsync(request,token);await VerifyAsync(response,token);
        }
        /// <summary>
        /// Logout for the current entity operation.
        /// </summary>
        /// <param name="token">The token used by this operation.</param>
        internal async Task LogoutAsync(CancellationToken token=default)
        {
            using var response=await _http.PostAsync("api/accounts/logout",null,token);await VerifyAsync(response,token);
        }
        /// <summary>Uploads a locally normalized image and returns its server-generated media identifier.</summary>
        internal async Task<Guid> UploadImageAsync(byte[] image,CancellationToken token=default)
        {
            using var body=new ByteArrayContent(image);body.Headers.ContentType=new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            using var response=await _http.PostAsync("api/media",body,token);await VerifyAsync(response,token);return await ReadIdentifierAsync(response,token);
        }
        /// <summary>Downloads private media through the authenticated connection.</summary>
        internal async Task<byte[]> GetImageAsync(Guid mediaId,CancellationToken token=default)
        {
            using var response=await _http.GetAsync($"api/media/{mediaId}",token);await VerifyAsync(response,token);return await response.Content.ReadAsByteArrayAsync(token);
        }
        /// <summary>Rejects malformed creation responses as a controlled operation error.</summary>
        private static async Task<Guid> ReadIdentifierAsync(HttpResponseMessage response,CancellationToken token)
        {
            try{var id=await response.Content.ReadFromJsonAsync<Guid>(cancellationToken:token);if(id==Guid.Empty)throw new InvalidOperationException("El servidor no devolvió un identificador válido.");return id;}
            catch(JsonException error){throw new InvalidOperationException("La API devolvió una respuesta inesperada al guardar. Revisa las versiones de cliente y servidor.",error);}
        }
        private static void Validate<T>(T data) where T:struct
        {
            object value=data;var results=new List<ValidationResult>();
            if(!Validator.TryValidateObject(value,new ValidationContext(value),results,true))throw new ValidationException(string.Join(Environment.NewLine,results.Select(x=>x.ErrorMessage)));
        }
        /// <summary>
        /// Converts an unsuccessful HTTP response into a controlled operation error.
        /// </summary>
        /// <param name="response">The response used by this operation.</param>
        /// <param name="token">The token used by this operation.</param>
        private static async Task VerifyAsync(HttpResponseMessage response,CancellationToken token)
        {
            if(response.IsSuccessStatusCode)return;
            if(response.StatusCode==HttpStatusCode.Unauthorized)throw new InvalidOperationException("Credenciales incorrectas o sesión terminada. Inicia sesión nuevamente.");
            var text=await response.Content.ReadAsStringAsync(token);string message="La API no pudo completar la operación.";
            try
            {
                using var document=JsonDocument.Parse(text);var root=document.RootElement;
                if(root.TryGetProperty("detail",out var detail))message=detail.GetString()??message;
                else if(root.TryGetProperty("errors",out var errors))message=string.Join(Environment.NewLine,errors.EnumerateObject().SelectMany(x=>x.Value.EnumerateArray().Select(v=>v.GetString())));
            }
            catch(JsonException) { }
            throw new InvalidOperationException(response.StatusCode==(HttpStatusCode)429?"Demasiados intentos. Espera un minuto.":message);
        }
        /// <summary>
        /// Releases the HTTP connection and its retained session state.
        /// </summary>
        public void Dispose()=>_http.Dispose();
    #endregion Request Operations
}
