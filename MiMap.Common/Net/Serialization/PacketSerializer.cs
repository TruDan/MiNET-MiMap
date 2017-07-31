namespace MiMap.Common.Net.Serialization
{
    public static class PacketSerializer
    {
        public static string Encode<TPacket>(this TPacket packet) where TPacket : IPacket
        {
            return MiMapJsonConvert.SerializeObject(packet);
        }

        public static int GetPacketId(this string rawPacketJson)
        {
            return MiMapJsonConvert.DeserializeObject<NullPacket>(rawPacketJson).Id;
        }

        public static TPacket Decode<TPacket>(this string rawPacket) where TPacket : class, IPacket
        {
            return MiMapJsonConvert.DeserializeObject<TPacket>(rawPacket);
        }

    }
}
