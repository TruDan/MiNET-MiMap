using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using MiMap.Common.Configuration;
using Owin;

namespace MiMap.Web
{
    public class WebStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            contentTypeProvider.Mappings.Add(".json", "application/json");

            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = new PathString("/tiles"),
                FileSystem = new PhysicalFileSystem(MiMapConfig.Config.TilesDirectory),
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true,
                StaticFileOptions =
                {
                    ContentTypeProvider = contentTypeProvider
                }
            });

            app.UseFileServer(new FileServerOptions
            {
                FileSystem = new EmbeddedResourceFileSystem(typeof(MiMapWebServer).Assembly, GetType().Namespace + ".Content"),
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true
            });
        }
    }
}
