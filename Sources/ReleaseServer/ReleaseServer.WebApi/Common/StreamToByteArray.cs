using System.IO;
using System.Threading.Tasks;

namespace ReleaseServer.WebApi.Common
{
    public static class StreamToByteArray
    {
        public static async Task<byte[]> ToByteArrayAsync(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}