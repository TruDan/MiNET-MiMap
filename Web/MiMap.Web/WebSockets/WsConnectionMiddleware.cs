using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MiMap.Web.WebSockets
{
	public class WsConnectionMiddleware
	{
		private readonly RequestDelegate _next;

		private readonly WsConnectionManager _wsConnectionManager;


		public WsConnectionMiddleware(RequestDelegate next, WsConnectionManager wsConnectionManager)
		{
			_next = next;
			_wsConnectionManager = wsConnectionManager;
		}

		public async Task Invoke(HttpContext context)
		{

			if (!context.WebSockets.IsWebSocketRequest)
			{
				await _next.Invoke(context);
				return;
			}

			CancellationToken ct = context.RequestAborted;
			WebSocket currentSocket = await context.WebSockets.AcceptWebSocketAsync();
			var socketId = Guid.NewGuid().ToString();

			var connection = _wsConnectionManager.AddSocket(currentSocket);

			while (true)
			{
				if (ct.IsCancellationRequested)
				{
					break;
				}

				var response = await _wsConnectionManager.ReceiveMessageAsync(currentSocket, ct);
				if (string.IsNullOrEmpty(response))
				{
					if (currentSocket.State != WebSocketState.Open)
					{
						break;
					}

					continue;
				}

				await connection.HandleReceiveAsync(response);
			}

			await _wsConnectionManager.RemoveSocket(socketId);

			await currentSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
			currentSocket.Dispose();
		}

	}
}
