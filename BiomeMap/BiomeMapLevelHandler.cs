using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Data;
using BiomeMap.Drawing;
using BiomeMap.Http;
using BiomeMap.Output;
using BiomeMap.Renderer;
using MiNET.Utils;
using MiNET.Worlds;
using Newtonsoft.Json;

namespace BiomeMap
{
    public class BiomeMapLevelHandler
    {

        protected IChunkRenderer[] Renderers;

        protected IRenderOutput[] Outputs;

        private Dictionary<byte, RegionPath> _biomes = new Dictionary<byte, RegionPath>();

        private Dictionary<ChunkCoordinates, DateTime> _chunkCache = new Dictionary<ChunkCoordinates, DateTime>();

        protected Level Level;

        private Timer _timer;
        private readonly object _sync = new object();

        public BiomeMapLevelHandler(Level level)
        {
            Level = level;
            Renderers = new IChunkRenderer[]
            {
                new BiomeChunkRenderer()
            };

            Outputs = new IRenderOutput[]
            {
                new BitmapOutput(),
            };
        }

        public void Start()
        {
            _timer = new Timer(Update, null, 1 * 1000, 10 * 1000);
        }

        protected void Update(object state)
        {
            if (!Monitor.TryEnter(_sync))
                return;

            try
            {
                var chunks = Level.GetLoadedChunks();
                var started = false;

                foreach (var chunk in chunks)
                {
                    var c = new ChunkCoordinates(chunk.x, chunk.z);

                    DateTime lastUpdate;
                    if (_chunkCache.TryGetValue(c, out lastUpdate))
                    {
                        if (DateTime.UtcNow - lastUpdate > TimeSpan.FromMinutes(2))
                        {
                            _chunkCache.Remove(c);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (!started)
                    {
                        OnChunkUpdateStart();
                        started = true;
                    }

                    OnChunkUpdate(chunk);
                    _chunkCache.Add(c, DateTime.UtcNow);
                }

                if (started)
                {
                    OnChunkUpdateEnd();
                }
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer = null;
        }

        private void OnChunkUpdateStart()
        {
            foreach (var output in Outputs)
            {
                output.OnUpdateStart();
            }
        }

        private void OnChunkUpdateEnd()
        {
            foreach (var output in Outputs)
            {
                output.OnUpdateEnd();
            }

            BiomeMapSocketServer.Broadcast("reload");
        }

        private RegionPath GetBiomeRegion(byte biome)
        {
            RegionPath path;
            if (!_biomes.TryGetValue(biome, out path))
            {
                path = new RegionPath();
                _biomes.Add(biome, path);
            }
            return path;
        }

        public Dictionary<byte, RegionPath> GetBiomeRegions()
        {
            return new Dictionary<byte, RegionPath>(_biomes);
        }

        public void OnChunkUpdate(ChunkColumn chunk)
        {
            /*for(int x = 0;x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    byte biome = chunk.GetBiome(x, z);

                    var region = GetBiomeRegion(biome);
                    region.AddPoint(chunk.x + x, chunk.z + z);
                }
            }*/

            //Console.WriteLine(JsonConvert.SerializeObject(_biomes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Path), Formatting.Indented));

            foreach (var renderer in Renderers)
            {
                var data = renderer.RenderChunk(chunk);

                foreach (var output in Outputs)
                {
                    output.WriteChunk(data);
                }
            }
        }

    }
}
