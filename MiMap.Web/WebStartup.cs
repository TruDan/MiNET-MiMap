using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using MiMap.Common.Configuration;
using MiMap.Web.Middleware;
using Owin;

namespace MiMap.Web
{
    public class WebStartup
    {
        public void Configuration(IAppBuilder app)
        {
#if DEBUG
            app.Use(typeof(NoCacheMiddleware));
#endif
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            contentTypeProvider.Mappings.Add(".json", "application/json");

            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/tiles"),
                FileSystem = new PhysicalFileSystem(MiMapConfig.Config.TilesDirectory),
#if DEBUG
                EnableDirectoryBrowsing = true,
#endif
                EnableDefaultFiles = true,
                StaticFileOptions =
                {
                    ContentTypeProvider = contentTypeProvider,
                }
            });

#if DEBUG
            var contentFileSystem = new PhysicalFileSystem("S:\\Development\\Projects\\MiNET-MiMap\\MiMap.Web\\Content");
#else
            var contentFileSystem =
                new EmbeddedResourceFileSystem(typeof(MiMapWebServer).Assembly, GetType().Namespace + ".Content");
#endif


            app.UseFileServer(new FileServerOptions
            {
                FileSystem = contentFileSystem,
#if DEBUG
                EnableDirectoryBrowsing = true,
#endif
                EnableDefaultFiles = true
            });
        }
    }
}
