using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiMap.Common.Net;

namespace MiMap.Web.WebSockets
{
	public class WsConnectionManager
	{
		private static ConcurrentDictionary<string, WsConnection> _sockets = new ConcurrentDictionary<string, WsConnection>();

		public WsConnection GetSocketById(string id)
		{
			return _sockets.FirstOrDefault(p => p.Key == id).Value;
		}

		public ConcurrentDictionary<string, WsConnection> GetAll()
		{
			return _sockets;
		}

		public string GetId(WebSocket socket)
		{
			return _sockets.FirstOrDefault(p => p.Value.WebSocket == socket).Key;
		}

		public WsConnection AddSocket(WebSocket socket)
		{
			var connection = new WsConnection(this, socket);
			_sockets.TryAdd(CreateConnectionId(), connection);
			return connection;
		}

		public async Task RemoveSocket(string id)
		{
			_sockets.TryRemove(id, out var wsConnection);

			await wsConnection.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketManager",
				CancellationToken.None);
		}

		private string CreateConnectionId()
		{
			return Guid.NewGuid().ToString();
		}



		public static void BroadcastTileUpdate(TileUpdatePacket update)
		{
			Parallel.ForEach(_sockets.Values.ToArray(), async c => await c.SendTileUpdate(update));
		}

		public static void BroadcastPacket(IPacket packet)
		{
				Parallel.ForEach(_sockets.Values.ToArray(), async c => await c.SendPacketAsync(packet));

		}


		public async Task BroadcastMessageAsync(string message)
		{
			foreach (var pair in _sockets.Values.ToArray())
			{
				if (pair.WebSocket.State == WebSocketState.Open)
					await SendMessageAsync(pair.WebSocket, message);
			}
		}

		internal async Task SendMessageAsync(WebSocket socket, string data, CancellationToken ct = default(CancellationToken))
		{
			var buffer = Encoding.UTF8.GetBytes(data);
			var segment = new ArraySegment<byte>(buffer);
			await socket.SendAsync(segment, WebSocketMessageType.Text, true, ct);
		}

		internal async Task<string> ReceiveMessageAsync(WebSocket socket, CancellationToken ct = default(CancellationToken))
		{
			var buffer = new ArraySegment<byte>(new byte[8192]);
			using (var ms = new MemoryStream())
			{
				WebSocketReceiveResult result;
				do
				{
					ct.ThrowIfCancellationRequested();

					result = await socket.ReceiveAsync(buffer, ct);
					ms.Write(buffer.Array, buffer.Offset, result.Count);
				}
				while (!result.EndOfMessage);

				ms.Seek(0, SeekOrigin.Begin);
				if (result.MessageType != WebSocketMessageType.Text)
				{
					return null;
				}

				// Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
				using (var reader = new StreamReader(ms, Encoding.UTF8))
				{
					return await reader.ReadToEndAsync();
				}
			}
		}
	}
}