using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BiomeMap.Shared.Net.Serialization
{
    public static class PacketSerializer
    {
        public static string Encode<TPacket>(this TPacket packet) where TPacket : IPacket
        {
            return MiMapJsonConvert.SerializeObject(packet);
        }

        public static byte GetPacketId(this string rawPacketJson)
        {
            return MiMapJsonConvert.DeserializeObject<NullPacket>(rawPacketJson).Id;
        }

        public static TPacket Decode<TPacket>(this string rawPacket) where TPacket : class, IPacket
        {
            return MiMapJsonConvert.DeserializeObject<TPacket>(rawPacket);
        }

    }
}
