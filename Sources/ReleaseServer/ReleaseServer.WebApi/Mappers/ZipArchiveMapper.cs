using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ZipArchiveMapper
    {
        public static ZipArchive ToZipArchive(this IFormFile formFile)
        {
            //TODO: Clarify whether it's dangerous, that the stream didn't get disposed 
            var stream = formFile.OpenReadStream();
            return new ZipArchive(stream);
        }
    }
}