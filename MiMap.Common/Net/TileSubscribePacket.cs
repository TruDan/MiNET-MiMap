namespace MiMap.Common.Net
{
    public class TileSubscribePacket : IPacket
    {
        public int Id { get; } = Protocol.TileSubscribe;

        public bool Subscribe { get; set; } = true;

        public int CurrentZoomLevel { get; set; } = 0;

    }
}
