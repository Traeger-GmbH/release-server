using System;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace ReleaseServer.WebApi.Mappers
{
    public  class ZipArchiveMapper : IDisposable
    {
        private Stream ZipStream;
        
        public ZipArchive FormFileToZipArchive(IFormFile formFile)
        {
            ZipStream = formFile.OpenReadStream();
            return new ZipArchive(ZipStream);
        }

        public void Dispose()
        {
            ZipStream.Dispose();
        }
    }
}