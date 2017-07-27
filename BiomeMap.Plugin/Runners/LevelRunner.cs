using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BiomeMap.Drawing;
using BiomeMap.Common.Data;
using BiomeMap.Drawing.Events;
using BiomeMap.Common.Net;
using BiomeMap.Common.Net.Data;
using BiomeMap.Web.Sockets;
using log4net;
using MiNET;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace BiomeMap.Plugin.Runners
{
    public class LevelRunner
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LevelRunner));

        public const int UpdateInterval = 500;
        public const int MaxChunksPerInterval = 1000;

        private readonly MiNetServer _server;

        public LevelMap Map { get; }

        private Level Level { get; set; }

        private bool _init = false;

        private readonly List<ChunkCoordinates> _renderedChunks = new List<ChunkCoordinates>();

        private readonly Timer _timer;
        private readonly object _updateSync = new object();


        public LevelRunner(MiNetServer server, LevelMap map)
        {
            _server = server;
            Map = map;

            Map.OnTileUpdated += MapOnOnTileUpdated;

            _timer = new Timer(DoUpdate);
        }

        private void MapOnOnTileUpdated(object sender, TileUpdateEventArgs e)
        {
            WsServer.BroadcastTileUpdate(new TileUpdatePacket()
            {
                LayerId = e.LayerId,
                Tile = new Tile()
                {
                    X = e.TileX,
                    Y = e.TileY,
                    Zoom = e.TileZoom
                }
            });
        }

        public void Start()
        {
            _timer.Change(UpdateInterval, UpdateInterval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void DoUpdate(object state)
        {
            if (!Monitor.TryEnter(_updateSync))
            {
                return;
            }

            try
            {
                UpdateLevel();
            }
            finally
            {
                Monitor.Exit(_updateSync);
            }
        }

        private void UpdateLevel()
        {
            TryGetLevel();
            if (Level == null) return;

            if (!_init)
            {
                _init = true;

                var chunkGen = Config.GetProperty("GenChunks", 64);
                if (chunkGen > 0)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        //for (int j = 0; j < chunkGen; j++)
                        //{
                        GenerateChunks(Level, chunkGen);
                        //}
                    });
                }
            }
            return;
            var sw = Stopwatch.StartNew();

            var chunks = Level.GetLoadedChunks();

            if (chunks.Length == 0)
                return;

            var i = 0;
            foreach (var chunk in chunks)
            {
                if (i >= MaxChunksPerInterval)
                    break;

                var coords = new ChunkCoordinates(chunk.x, chunk.z);
                if (_renderedChunks.Contains(coords))
                    continue;

                RenderChunk(chunk);

                i++;
            }

            if (i > 0)
            {
                Log.InfoFormat("Updated {0} chunks in {1}ms", i, sw.ElapsedMilliseconds);
            }
        }

        private void GenerateChunks(Level level, int r)
        {

            for (int dx = -r; dx <= r; dx++)
            {
                for (int dz = -r; dz <= r; dz++)
                {

                    var coords = new ChunkCoordinates(dx, dz);
                    //ThreadPool.QueueUserWorkItem(c => level.GetChunk((ChunkCoordinates) c), coords);

                    var chunk = level.GetChunk(coords);
                    RenderChunk(chunk);
                    (level.WorldProvider as ICachingWorldProvider)?.ClearCachedChunks();

                }
            }
        }

        private void RenderChunk(ChunkColumn chunk)
        {
            _renderedChunks.Add(new ChunkCoordinates(chunk.x, chunk.z));
            chunk.RecalcHeight();

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    var meta = GetColumnMeta(chunk, x, z);
                    Map.UpdateBlockColumn(meta);
                }
            }
        }

        private BlockColumnMeta GetColumnMeta(ChunkColumn chunk, int x, int z)
        {
            var pos = new BlockPosition((chunk.x << 4) + x, (chunk.z << 4) + z);

            var y = chunk.GetHeight(x, z);
            //var highestBlock = Level.GetBlock(pos.X, y, pos.Z);
            var highestBlock = GetHighestBlock(pos.X, pos.Z);

            if (highestBlock == null)
            {
                return new BlockColumnMeta()
                {
                    Position = pos,
                    Height = 0,
                    BiomeId = 0,
                    BlockId = 0,
                    LightLevel = 0
                };
            }

            //_skyLightCalculations.Calculate(Level, highestBlock);

            //if (highestBlock.LightLevel > 0)
            //{
            //    BlockLightCalculations.Calculate(Level, highestBlock);
            //}

            return new BlockColumnMeta()
            {
                Position = pos,
                Height = (byte)highestBlock.Coordinates.Y,
                BiomeId = highestBlock.BiomeId,
                BlockId = highestBlock.Id,
                LightLevel = chunk.GetSkylight(x, y, z)
            };
        }

        private Block GetHighestBlock(int x, int z)
        {
            for (int y = 255; y > 0; y--)
            {
                var block = Level.GetBlock(x, y, z);
                //if (!block.IsTransparent)
                if (!(block is Air))
                {
                    return block;
                }
            }
            return null;
        }

        private void TryGetLevel()
        {
            if (Level != null) return;

            var lm = _server.LevelManager;
            if (lm == null) return;

            Level = lm.Levels.FirstOrDefault(l => l != null &&
                                                  l.LevelId.Equals(Map.Config.LevelId,
                                                      StringComparison.InvariantCultureIgnoreCase));

            if (Level != null)
            {
                WsServer.BroadcastPacket(new LevelMetaPacket()
                {
                    LevelId = Level.LevelId,
                    Meta = Map.Meta
                });
            }
        }
    }
}
