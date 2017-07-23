using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Shared.Net;
using BiomeMap.Shared.Net.Serialization;
using Fleck;

namespace BiomeMap.Plugin.Net
{
    public class WsConnection
    {

        private bool _liveUpdates = true;
        private int _zoomLevel = 0;

        private readonly IWebSocketConnection _connection;

        public WsConnection(IWebSocketConnection connection)
        {
            _connection = connection;
            _connection.OnMessage += OnMessage;
        }

        private void OnMessage(string msg)
        {
            var id = msg.GetPacketId();
            if (id == Protocol.TileSubscribe)
            {
                var packet = msg.Decode<TileSubscribePacket>();
                _liveUpdates = packet.Subscribe;
                _zoomLevel = packet.CurrentZoomLevel;
            }
        }

        public void SendTileUpdate(TileUpdatePacket packet)
        {
            if (!_liveUpdates) return;
            if (packet.Tile.Zoom != _zoomLevel) return;

            SendPacket(packet);
        }

        public void SendPacket(IPacket packet)
        {
            _connection.Send(packet.Encode());
        }
    }
}
