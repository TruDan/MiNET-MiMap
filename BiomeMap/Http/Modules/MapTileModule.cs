using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Http.Response;
using Nancy;
using ResponseExtensions = BiomeMap.Http.Response.ResponseExtensions;

namespace BiomeMap.Http.Modules
{
    public class MapTileModule : NancyModule
    {
        public string OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RenderMaps");

        public MapTileModule()
        {
            Get["/tile/{y}/{x}/{z}.png"] = GetTile;
        }

        private dynamic GetTile(dynamic o)
        {
            var z = int.Parse(o.z);
            var x = int.Parse(o.x);
            var y = int.Parse(o.y);

            var path = Path.Combine(OutputPath, z.ToString(), x + "_" + y + ".png");

            if (File.Exists(path))
            {
                byte[] img = File.ReadAllBytes(path);
                return ResponseExtensions.FromByteArray(Response, img, "image/png");
            }
            else
            {
                using (var img = new Bitmap(1, 1))
                {
                    img.SetPixel(0, 0, Color.Black);
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);

                        return ResponseExtensions.FromByteArray(Response, ms.ToArray(), "image/png");
                    }
                }
            }
        }
    }
}
