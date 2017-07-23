using System;
using System.Collections.Generic;
using System.Text;

namespace BiomeMap.Shared.Net
{
    public sealed class NullPacket : IPacket
    {
        public byte Id { get; } = Protocol.Null;

    }
}
