using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MiMap.Web.Middleware;

namespace MiMap.Web.WebSockets
{
	public static class WsManagerExtensions
	{
		public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
		{
			services.AddTransient<WsConnectionManager>();

			//foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
			//{
			//	if (type.GetTypeInfo().BaseType == typeof(WsConnection))
			//	{
			//		services.AddSingleton(type);
			//	}
			//}

			return services;
		}

		public static IApplicationBuilder MapWebSocketManager(this IApplicationBuilder app)
		{
			app.UseWebSockets();

			app.UseMiddleware<WsConnectionMiddleware>();

			return app;
		}
	}
}
