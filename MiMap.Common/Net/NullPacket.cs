using System.Collections.Generic;

namespace MiMap.Common.Net
{
    public sealed class NullPacket : IPacket
    {
        public int Id { get; set; } = Protocol.Null;

    }
}
