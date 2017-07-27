using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.StaticFiles.ContentTypes;
using Owin;

namespace BiomeMap.Plugin.Net
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
                FileSystem = new PhysicalFileSystem(BiomeMapPlugin.Config.TilesDirectory),
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true,
                StaticFileOptions =
                {
                    ContentTypeProvider = contentTypeProvider
                }
            });

            app.UseFileServer(new FileServerOptions
            {
                FileSystem = new EmbeddedResourceFileSystem(typeof(MiMapWebServer).Assembly, "BiomeMap.Plugin.Content"),
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true
            });
        }
    }
}
