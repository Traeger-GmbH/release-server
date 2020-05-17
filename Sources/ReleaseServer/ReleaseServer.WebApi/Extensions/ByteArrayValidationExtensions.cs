using System.Text;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Extensions
{
    public static class ByteArrayValidationExtensions
    {
        public static bool IsAValidJson<T>(this byte[] byteArray, out string exceptionMessage)
        {
            exceptionMessage = null;

            try
            { 
                JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(byteArray));
            }
            //Json has an invalid format not readable
            catch (JsonReaderException je)
            {
                exceptionMessage = je.Message;
                return false;
            }
            
            //Json has an invalid structure and a serialization is not possible
            catch (JsonSerializationException je)
            {
                exceptionMessage = je.Message;
                return false;
            }
            
            return true;
        }
    }
}