using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BiomeMap.Shared.Net.Serialization
{
    public static class PacketSerializer
    {
        private static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings()
        {

            ContractResolver = new CamelCasePropertyNamesContractResolver()
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        public static string Encode<TPacket>(this TPacket packet) where TPacket : IPacket
        {
            return JsonConvert.SerializeObject(packet, JsonSerializerSettings);
        }

        public static byte GetPacketId(this string rawPacketJson)
        {
            return JsonConvert.DeserializeObject<NullPacket>(rawPacketJson, JsonSerializerSettings).Id;
        }

        public static TPacket Decode<TPacket>(this string rawPacket) where TPacket : class, IPacket
        {
            return JsonConvert.DeserializeObject<TPacket>(rawPacket, JsonSerializerSettings);
        }

    }
}
