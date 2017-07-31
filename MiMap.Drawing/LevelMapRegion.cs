using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MiMap.Common;
using MiMap.Common.Data;
using MiMap.Drawing;

namespace BiomeMap.Drawing
{
    public class LevelMapRegion
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LevelMapRegion));

        private const int Size = 32 * 16;

        public LevelMap Map { get; }
        public RegionPosition Position { get; }

        //public BlockColumnMeta[,] Blocks { get; private set; } = new BlockColumnMeta[Size, Size];

        private Dictionary<int, Dictionary<int, BlockColumnMeta>> Blocks { get; set; } = new Dictionary<int, Dictionary<int, BlockColumnMeta>>();

        private string FilePath { get; }
        private readonly object _ioSync = new object();

        private bool _isDirty = false;

        public LevelMapRegion(LevelMap map, RegionPosition pos)
        {
            Map = map;
            Position = pos;
            FilePath = Path.Combine(map.TilesDirectory, "meta",
                $"{Position.X}_{Position.Z}.json");

            Load();
        }

        private void Load()
        {
            lock (_ioSync)
            {
                if (File.Exists(FilePath))
                {
                    try
                    {
                        using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite))
                        {
                            //using (var gs = new GZipStream(fs, CompressionMode.Decompress))
                            {
                                using (var sr = new StreamReader(fs))
                                {
                                    var json = sr.ReadToEnd();
                                    var blocksArray = MiMapJsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, BlockColumnMeta>>>(json);

                                    Blocks = blocksArray;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Exception loading region meta for " + Position, ex);
                        //Blocks = new BlockColumnMeta[Size, Size];
                    }
                }
            }
        }

        public void Save()
        {
            lock (_ioSync)
            {
                if (!_isDirty) return;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    using (var fs = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        //using (var gs = new GZipStream(fs, CompressionMode.Compress))
                        {
                            using (var sr = new StreamWriter(fs))
                            {
                                //BlockColumnMeta[,] blocks = (BlockColumnMeta[,])Blocks.Clone();

                                var json = MiMapJsonConvert.SerializeObject(Blocks);
                                sr.Write(json);
                            }
                        }
                    }
                    _isDirty = false;
                }
                catch (Exception ex)
                {
                    Log.Error("Exception saving region meta for " + Position, ex);
                }
            }
        }

        public void Update(BlockColumnMeta update)
        {
            var pos = update.Position;
            if (!Position.GetBlockBounds().Contains(pos))
            {
                Log.InfoFormat("Region {0} with bounds {1} doesn't contain block {2}", Position, Position.GetBlockBounds(), pos);
                return;
            }

            var x = pos.X % Size;
            var z = pos.Z % Size;

            x = x < 0 ? Size + x : x;
            z = z < 0 ? Size + z : z;

            lock (_ioSync)
            {
                //Log.InfoFormat("Updating block {0},{1} @ {2}", x, z, update.Position);
                if (!Blocks.ContainsKey(x))
                {
                    Blocks.Add(x, new Dictionary<int, BlockColumnMeta>());
                }

                Blocks[x][z] = update;
                _isDirty = true;
            }

        }

        public BlockColumnMeta GetBlockData(BlockPosition pos)
        {
            if (!Position.GetBlockBounds().Contains(pos))
            {
                return null;
            }

            var x = pos.X % (1 << 9);
            var z = pos.Z % (1 << 9);

            x = x < 0 ? Size + x : x;
            z = z < 0 ? Size + z : z;

            lock (_ioSync)
            {
                if (!Blocks.ContainsKey(x))
                {
                    return null;
                }
                if (!Blocks[x].ContainsKey(z))
                {
                    return null;
                }
                return Blocks[x][z];
            }
        }
    }
}
