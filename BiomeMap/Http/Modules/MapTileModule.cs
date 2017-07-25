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
using BiomeMap.Output;
using BiomeMap.Shared;
using Nancy;
using Newtonsoft.Json;
using ResponseExtensions = BiomeMap.Http.Response.ResponseExtensions;

namespace BiomeMap.Http.Modules
{
    public class MapTileModule : NancyModule
    {
        public string OutputPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RenderMaps");

        public MapTileModule()
        {
            Get["/tile/{y}/{x}/{z}.png"] = GetRegionTile;
            Get["/head/{player}.png"] = GetPlayerHead;
            Get["/regions/{level}.json"] = GetRegions;
            Get["/config.json"] = GetConfig;
        }

        private dynamic GetConfig(dynamic o)
        {
            return MiMapJsonConvert.SerializeObject(new
            {
                Levels = BiomeMapPlugin.GetLevelNames(),
                TileSize = BitmapOutput.RegionSize * 16
            });
        }

        private dynamic GetRegions(dynamic o)
        {
            BiomeMapLevelHandler handler = BiomeMapPlugin.GetMapHandler(o.level);
            if (handler == null) return null;

            var regions = handler.GetBiomeRegions();

            return MiMapJsonConvert.SerializeObject(regions.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Points.Select(r => r.Select(p => new[] { p.X, p.Y }))));
        }

        private dynamic GetPlayerHead(dynamic o)
        {
            var player = o.player;

            var path = Path.Combine(OutputPath, "PlayerHead", player + ".png");

            if (File.Exists(path))
            {
                byte[] img = File.ReadAllBytes(path);
                return ResponseExtensions.FromByteArray(Response, img, "image/png");
            }
            return null;
        }

        private dynamic GetChunkTile(dynamic o)
        {
            var z = int.Parse(o.z);
            var x = int.Parse(o.x);
            var y = int.Parse(o.y);

            var path = Path.Combine(OutputPath, "Raw", x + "_" + y + ".png");

            if (File.Exists(path))
            {
                byte[] img = File.ReadAllBytes(path);
                return ResponseExtensions.FromByteArray(Response, img, "image/png");
            }
            return null;
        }
        private dynamic GetRegionTile(dynamic o)
        {
            var z = int.Parse(o.z);
            var x = int.Parse(o.x);
            var y = int.Parse(o.y);

            var path = Path.Combine(OutputPath, "Region", "r." + x + "_" + y + ".png");

            if (File.Exists(path))
            {
                byte[] img = File.ReadAllBytes(path);
                return ResponseExtensions.FromByteArray(Response, img, "image/png");
            }
            return null;
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
