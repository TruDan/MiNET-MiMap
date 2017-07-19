using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BiomeMap.Drawing;
using BiomeMap.Shared.Data;
using MiNET;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace BiomeMap.Runners
{
    public class LevelRunner
    {
        public const int UpdateInterval = 1000;
        public const int MaxChunksPerInterval = 10;

        private readonly MiNetServer _server;

        public LevelMap Map { get; }

        private Level Level { get; set; }

        private readonly List<ChunkCoordinates> _renderedChunks = new List<ChunkCoordinates>();

        private Timer _timer;
        private readonly object _updateSync = new object();

        public LevelRunner(MiNetServer server, LevelMap map)
        {
            _server = server;
            Map = map;
            _timer = new Timer(DoUpdate);
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

            var chunks = Level.GetLoadedChunks();

            if (chunks.Length == 0)
                return;

            var i = 0;
            foreach (var chunk in chunks)
            {
                if (i >= MaxChunksPerInterval)
                    return;

                var coords = new ChunkCoordinates(chunk.x, chunk.z);
                if (_renderedChunks.Contains(coords))
                    continue;

                RenderChunk(chunk);
                _renderedChunks.Add(coords);

                i++;
            }
        }

        private void RenderChunk(ChunkColumn chunk)
        {
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

            var highestBlock = GetHighestBlock(pos.X, pos.Z);
            if (highestBlock == null)
            {
                return new BlockColumnMeta()
                {
                    Position = pos,
                    Height = 0,
                    BiomeId = 0,
                    BlockId = 0
                };
            }

            return new BlockColumnMeta()
            {
                Position = pos,
                Height = (byte)highestBlock.Coordinates.Y,
                BiomeId = highestBlock.BiomeId,
                BlockId = highestBlock.Id
            };
        }

        private Block GetHighestBlock(int x, int z)
        {
            for (int y = 255; y > 0; y--)
            {
                var block = Level.GetBlock(x, y, z);
                if (!block.IsTransparent)
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
        }
    }
}
