using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using Topshelf;


// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]

namespace MiNET.Server
{
    public class MiNetService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MiNetServer));

        private MiNetServer _server;

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        private void Start()
        {

            var assembly = Assembly.GetExecutingAssembly().GetName().CodeBase;
            //string rawPath = Path.GetDirectoryName(assembly);
            Log.Info(assembly);

            Log.Info("Starting MiNET as user " + Environment.UserName);
            _server = new MiNetServer();
            //_server.LevelManager = new SpreadLevelManager(Environment.ProcessorCount * 4);
            _server.StartServer();
        }

        /// <summary>
        ///     Stops this instance.
        /// </summary>
        private void Stop()
        {
            Log.Info("Stopping MiNET");
            _server.StopServer();
        }

        /// <summary>
        ///     The programs entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            if (IsRunningOnMono())
            {
                var service = new MiNetService();
                service.Start();
                Console.WriteLine("MiNET running. Press <enter> to stop service.");
                Console.ReadLine();
                service.Stop();
            }
            else
            {
                HostFactory.Run(host =>
                {
                    host.Service<MiNetService>(s =>
                    {
                        s.ConstructUsing(construct => new MiNetService());
                        s.WhenStarted(service => service.Start());
                        s.WhenStopped(service => service.Stop());
                    });

                    host.RunAsLocalService();
                    host.SetDisplayName("MiNET Service");
                    host.SetDescription("MiNET Minecraft Pocket Edition server.");
                    host.SetServiceName("MiNET");
                });
            }

            Console.ReadLine();
        }

        /// <summary>
        ///     Determines whether is running on mono.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}
