using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BiomeMap.Common
{
    public static class MiMapJsonConvert
    {
        public const bool PrettyPrint = false;

        private static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings()
        {

            ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        public static string SerializeObject(object obj, bool prettyPrint = PrettyPrint)
        {
            return JsonConvert.SerializeObject(obj, prettyPrint ? Formatting.Indented : Formatting.None, DefaultSettings);
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, DefaultSettings);
        }

    }
}
