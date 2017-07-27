namespace BiomeMap.Common.Net
{
    public sealed class NullPacket : IPacket
    {
        public byte Id { get; } = Protocol.Null;

    }
}
