using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MiMap.Web
{
	public class Program
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

		public static void Main(string[] args)
		{
			Log.InfoFormat("Starting MiMap Web Server with args: {0}", string.Join(" ", args));

			using (var webServer = new MiMapWebServer())
			{
				webServer.Start();
				Log.InfoFormat("MiMap Web Server Started");
				Console.ReadLine();
			}

			Log.InfoFormat("MiMap Web Server Stopped");
			Console.ReadLine();
		}
	}
}
