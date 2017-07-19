using System.IO;
using System.IO.Compression;
using System.Reflection;
using BiomeMap.Shared;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BiomeMap.Web.Controllers
{
    public class ConfigController : Controller
    {
        private static BiomeMapWebConfig WebConfig { get; }

        private static BiomeMapConfig BiomeMapConfig { get; }

        static ConfigController()
        {
            WebConfig = GetWebConfig();

            BiomeMapConfig = GetBiomeMapConfig();

        }

        private static BiomeMapWebConfig GetWebConfig()
        {
            var configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.json");

            if (!System.IO.File.Exists(configPath))
            {
                var newConfig = new BiomeMapWebConfig();
                var newConfigJson = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                System.IO.File.WriteAllText(configPath, newConfigJson);

                return newConfig;
            }

            var json = System.IO.File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<BiomeMapWebConfig>(json);
        }

        private static BiomeMapConfig GetBiomeMapConfig()
        {
            var configPath = WebConfig.ConfigPath;

            if (!System.IO.File.Exists(configPath))
            {
                return new BiomeMapConfig();
            }

            var json = System.IO.File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<BiomeMapConfig>(json);
        }


        [HttpGet("config.json")]
        public IActionResult Get()
        {
            return Json(BiomeMapConfig);
        }

        [HttpGet("tiles/{levelId}/{layer}/{zoomLevel:int}/{x:int}_{z:int}.png")]
        public IActionResult GetTile(string levelId, string layer, int zoomLevel, int x, int z)
        {
            var path = Path.Combine(BiomeMapConfig.TilesDirectory, levelId, layer, zoomLevel.ToString(),
                x + "_" + z + ".png");

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "image/png");
        }

        [HttpGet("meta/{levelId}.json")]
        public IActionResult GetLevelMeta(string levelId)
        {
            var path = Path.Combine(BiomeMapConfig.TilesDirectory, levelId, "meta.json");

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var json = JsonConvert.DeserializeObject<LevelMeta>(System.IO.File.ReadAllText(path));
            return Json(json);
        }
    }
}
