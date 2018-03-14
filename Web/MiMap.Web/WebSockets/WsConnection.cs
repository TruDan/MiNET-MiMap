using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiMap.Common.Net;
using MiMap.Common.Net.Serialization;
using MiMap.Web.Middleware;

namespace MiMap.Web.WebSockets
{
	public class WsConnection
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(WsConnection));

		protected WsConnectionManager WsConnectionManager { get; }
		public WebSocket WebSocket { get; }

		private bool _liveUpdates = true;
		private int _zoomLevel = 0;

		public WsConnection(WsConnectionManager wsConnectionManager, WebSocket webSocket)
		{
			WsConnectionManager = wsConnectionManager;
			WebSocket = webSocket;
		}

		protected async Task OnMessage(string msg)
		{
			var id = msg.GetPacketId();
			Log.InfoFormat("Received Packet {1}: {0}", msg, id);
			if (id == Protocol.TileSubscribe)
			{
				var packet = msg.Decode<TileSubscribePacket>();
				_liveUpdates = packet.Subscribe;
				_zoomLevel = packet.CurrentZoomLevel;

				Log.InfoFormat("Received Tile Subscribe Request: Subscribe: {0}, ZoomLevel: {1}", packet.Subscribe, packet.CurrentZoomLevel);
			}
		}

		public async Task SendTileUpdate(TileUpdatePacket packet)
		{
			if (!_liveUpdates) return;
			if (packet.Tile.Zoom != _zoomLevel) return;

			await SendPacketAsync(packet);
		}

		public async Task SendPacketAsync(IPacket packet)
		{
			await SendMessageAsync(packet.Encode());
		}

		public async Task SendMessageAsync(string message)
		{
			await WsConnectionManager.SendMessageAsync(WebSocket, message);
		}

		public async Task HandleReceiveAsync(string msg)
		{
			await OnMessage(msg);
		}

	}
}