using System.Text;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Extensions
{
    public static class ByteArrayValidationExtensions
    {
        public static bool IsAValidJson(this byte[] byteArray, out string exceptionMessage)
        {
            exceptionMessage = null;

            try
            { 
                JsonConvert.DeserializeObject(Encoding.UTF8.GetString(byteArray));
            }
            catch (JsonReaderException je)
            {
                exceptionMessage = je.Message;
                return false;
            }
            
            return true;
        }
    }
}