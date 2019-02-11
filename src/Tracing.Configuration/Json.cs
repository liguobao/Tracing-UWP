using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tracing.Configuration
{
    public static class Json
    {
        public static async Task<T> ToObjectAsync<T>(string value)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(value,
                                        new JsonSerializerSettings
                                        {
                                            DefaultValueHandling = DefaultValueHandling.Ignore
                                        }));
        }

        public static async Task<string> StringifyAsync(object value)
        {
            return await Task.Run(() => JsonConvert.SerializeObject(value));
        }
    }
}
