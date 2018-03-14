using MiMap.Web.Widgets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using MiMap.Common.Configuration;
using MiMap.Web.Middleware;
using MiMap.Web.Utils;
using MiMap.Web.WebSockets;

namespace MiMap.Web
{
	public class MiMapWebServer : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MiMapWebServer));

		private WidgetManager WidgetManager { get; }
		private IWebHost _host;

		private short WebPort { get; }

		public string[] Urls { get; }

		public MiMapWebServer() : this(new MiMapWebServerConfig())
		{
		}

		public MiMapWebServer(MiMapWebServerConfig config) : this(config.InternalWebPort, config.Widgets)
		{
		}

		public MiMapWebServer(short webPort, MiMapWidgetConfig[] widgets)
		{
			WebPort = webPort;

			Urls = new[] { $"http://+:{WebPort}" };

			WidgetManager = new WidgetManager(widgets);

			_host = WebHost.CreateDefaultBuilder()
				.UseUrls(Urls)
				.ConfigureServices(ConfigureServices)
				.Configure(Configure)
				.Build();
		}

		public void Start()
		{
			if (!TryStartListener())
			{
				if (!TryAddUrlReservations())
				{
					Log.ErrorFormat("Unable to add url reservations for web server.");
					return;
				}

				if (!TryStartListener())
				{
					Log.ErrorFormat("Unable to start listener");
					return;
				}
			}

			Log.InfoFormat("Web server started.");
		}

		public void Stop()
		{
			_host.StopAsync().RunSynchronously();
			Log.InfoFormat("Web server stopped.");
		}


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc();
			services.AddWebSocketManager();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();

			app.UseMiddleware<NoCacheMiddleware>();

			var contentTypeProvider = new FileExtensionContentTypeProvider();

			if (!Directory.Exists(MiMapConfig.Config.TilesDirectory))
			{
				Directory.CreateDirectory(MiMapConfig.Config.TilesDirectory);
			}

			app.UseFileServer(new FileServerOptions()
			{
				RequestPath = new PathString("/tiles"),
				FileProvider = new PhysicalFileProvider(MiMapConfig.Config.TilesDirectory),
#if DEBUG
				EnableDirectoryBrowsing = true,
#endif
				EnableDefaultFiles = true,
				StaticFileOptions =
				{
					ContentTypeProvider = contentTypeProvider,
				}
			});
			
			// WebSockets
			app.MapWebSocketManager();

			// Widget Stuff
			WidgetManager.ConfigureHttp(app);

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private bool TryStartListener()
		{

			try
			{
				_host.Start();
				return true;
			}
			catch (TargetInvocationException e)
			{
				var innerException = e.InnerException as HttpListenerException;
				if (innerException?.ErrorCode == 5) // 5 == AccessDenied
				{
					return false;
				}

				throw;
			}
		}

		private bool TryAddUrlReservations()
		{
			foreach (var url in Urls)
			{
				if (!NetSh.AddUrlAcl(url, WindowsIdentity.GetCurrent().Name))
				{
					return false;
				}
			}

			return true;
		}

		public void Dispose()
		{
			_host?.Dispose();
		}
	}
}
