using System.Buffers.Binary;
using CoffeManager.Infrastructure.Database;
namespace CoffeManager.Models.Media.Classes;
/// <summary>Stores private image files under server-generated identifiers and SQL ownership metadata.</summary>
internal sealed class Media(SqlConnectionFactory connections,IConfiguration configuration,IWebHostEnvironment environment)
{
    #region Operation Data
        internal const int MaxImageBytes=2*1024*1024;
        private string Root=>Path.GetFullPath(configuration["Media:RootPath"]??Path.Combine(environment.ContentRootPath,"App_Data","Media"));
    #endregion Operation Data
    #region Task Media Operations
        /// <summary>Checks a bounded PNG/JPEG image, writes its file and persists ownership metadata.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="data">The interaction fields supplied for this request.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        internal async Task<Guid> UploadAsync(Guid ownerId,byte[] data,CancellationToken cancellationToken)
        {
            var type=InspectImage(data);var id=Guid.NewGuid();Directory.CreateDirectory(Root);var path=Path.Combine(Root,id.ToString("N")+".bin");
            try
            {
                await File.WriteAllBytesAsync(path,data,cancellationToken);
                await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();command.CommandText="INSERT dbo.MediaFiles(MediaId,OwnerUserId,ContentType,SizeBytes) VALUES(@id,@owner,@type,@size)";
                command.Id("@id",id);command.Id("@owner",ownerId);command.Text("@type",type,30);command.Parameters.Add("@size",System.Data.SqlDbType.Int).Value=data.Length;await command.ExecuteNonQueryAsync(cancellationToken);return id;
            }
            catch{File.Delete(path);throw;}
        }
        /// <summary>Resolves only an image belonging to the authenticated owner scope.</summary>
        /// <param name="ownerId">The owner identifier resolved from the authenticated session.</param>
        /// <param name="cancellationToken">Cancels the asynchronous operation.</param>
        /// <param name="mediaId">The server-generated identifier of the private image.</param>
        internal async Task<(string Path,string Type)?> GetAsync(Guid ownerId,Guid mediaId,CancellationToken cancellationToken)
        {
            await using var connection=await connections.OpenAsync(cancellationToken);using var command=connection.CreateCommand();command.CommandText="SELECT ContentType FROM dbo.MediaFiles WHERE MediaId=@id AND OwnerUserId=@owner";command.Id("@id",mediaId);command.Id("@owner",ownerId);var type=await command.ExecuteScalarAsync(cancellationToken) as string;
            var path=Path.Combine(Root,mediaId.ToString("N")+".bin");return type is null||!File.Exists(path)?null:(path,type);
        }
        /// <summary>Accepts bounded raster formats with valid dimension headers; SVG and arbitrary file extensions are not served.</summary>
        internal static string InspectImage(byte[] data)
        {
            if(data.Length<24||data.Length>MaxImageBytes)throw new ArgumentException("La imagen debe ser PNG o JPEG y pesar como máximo 2 MB.");
            if(data.AsSpan(0,8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10})&&data.AsSpan(12,4).SequenceEqual("IHDR"u8))
            {
                Dimensions(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(16,4)),BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(20,4)));return "image/png";
            }
            if(data[0]==255&&data[1]==216&&data[^2]==255&&data[^1]==217)
            {
                var i=2;while(i+4<data.Length)
                {
                    if(data[i++]!=255)break;while(i<data.Length&&data[i]==255)i++;if(i>=data.Length)break;var marker=data[i++];if(marker is 0xD8 or 0xD9||marker is >=0xD0 and <=0xD7)continue;
                    if(i+2>data.Length)break;var length=BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(i,2));if(length<2||i+length>data.Length)break;
                    if(marker is 0xC0 or 0xC1 or 0xC2){if(length<8)break;Dimensions(BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(i+5,2)),BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(i+3,2)));return "image/jpeg";}i+=length;
                }
            }
            throw new ArgumentException("El archivo no tiene un encabezado PNG o JPEG válido.");
        }
        private static void Dimensions(uint width,uint height){if(width==0||height==0||width>6000||height>6000||(long)width*height>20000000)throw new ArgumentException("Las dimensiones de la imagen no son válidas. Usa una imagen de hasta 6000 píxeles y 20 megapíxeles.");}
    #endregion Task Media Operations
}
