using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using log4net;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiMap.AnvilTileGenerator.Worlds.Lighting
{
    public class SkyLightCalculations
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BlockLightCalculations));


        // Debug tracking, don't enable unless you really need to "see it".
        
        public ConcurrentDictionary<BlockCoordinates, int> Visits { get; } = new ConcurrentDictionary<BlockCoordinates, int>();
        public long StartTimeInMilliseconds { get; set; }

        ConcurrentDictionary<ChunkColumn, bool> _visitedColumns = new ConcurrentDictionary<ChunkColumn, bool>();

        public SkyLightCalculations()
        {
        }

        public static void Calculate(World level)
        {
            var chunks = level.GetLoadedChunks().OrderBy(column => column.x).ThenBy(column => column.z);

            _chunkCount = chunks.Count();

            if (_chunkCount == 0) return;

            //CheckIfSpawnIsMiddle(chunks, level.SpawnPoint.GetCoordinates3D());

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var pair in chunks)
            {
                pair.RecalcHeight();
            }
            Log.Debug($"Recalc height for {_chunkCount} chunks, {_chunkCount * 16 * 16 * 128} blocks. Time {sw.ElapsedMilliseconds}ms");


            var calculator = new SkyLightCalculations();
            
            sw.Restart();
            
            calculator.StartTimeInMilliseconds = Environment.TickCount;

            var t0 = Task.Run(() =>
            {
                var pairs = chunks.OrderBy(pair => pair.x).ThenBy(pair => pair.z).Where(chunk => chunk.x <= 0).OrderByDescending(pair => pair.x).ThenBy(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            var t5 = Task.Run(() =>
            {
                var pairs = chunks.OrderByDescending(pair => pair.x).ThenBy(pair => pair.z).Where(chunk => chunk.x > 0).OrderBy(pair => pair.x).ThenByDescending(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            var t1 = Task.Run(() =>
            {
                var pairs = chunks.OrderBy(pair => pair.x).ThenBy(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            var t2 = Task.Run(() =>
            {
                var pairs = chunks.OrderByDescending(pair => pair.x).ThenByDescending(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            var t3 = Task.Run(() =>
            {
                var pairs = chunks.OrderByDescending(pair => pair.x).ThenBy(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            var t4 = Task.Run(() =>
            {
                var pairs = chunks.OrderBy(pair => pair.x).ThenByDescending(pair => pair.z).ToArray();
                calculator.CalculateSkyLights(level, pairs);
            });

            Task.WaitAll(t0, t1, t2, t3, t4, t5);

            Log.Debug($"Recalc skylight for {_chunkCount}({_chunkCount}) chunks, {_chunkCount * 16 * 16 * 128:N0} blocks. Time {sw.ElapsedMilliseconds}ms");
            
            //foreach (var chunk in chunks)
            //{
            //	calculator.ShowHeights(chunk);
            //}

            //var chunkColumn = chunks.First(column => column.x == -1 && column.z == 0 );
            //if (chunkColumn != null)
            //{
            //	Log.Debug($"Heights:\n{Package.HexDump(chunkColumn.height)}");
            //	Log.Debug($"skylight.Data:\n{Package.HexDump(chunkColumn.skyLight.Data, 64)}");
            //}
        }

        private int CalculateSkyLights(World level, ChunkColumn[] chunks)
        {
            var calcCount = 0;
            var calcTime = new Stopwatch();
            var lastCount = 0;

            foreach (var chunk in chunks)
            {
                if (!_visitedColumns.TryAdd(chunk, true)) continue;

                if (chunk == null) continue;
                if (chunk.isAllAir) continue;

                calcTime.Restart();
                if (RecalcSkyLight(chunk, level))
                {
                    calcCount++;

                    var elapsedMilliseconds = calcTime.ElapsedMilliseconds;
                    var c = Visits.Sum(pair => pair.Value);
                    if (elapsedMilliseconds > 0) Log.Debug($"Recalc skylight for #{calcCount} (air={chunk.isAllAir}) chunks. Time {elapsedMilliseconds}ms and {c - lastCount} visits");
                    lastCount = c;
                    //PrintVisits();
                }
            }

            Log.Debug($"Recalc skylight for #{calcCount} chunk.");

            return calcCount;
        }

        public bool RecalcSkyLight(ChunkColumn chunk, World level)
        {
            if (chunk == null) return false;

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    if (chunk.isAllAir && !IsOnChunkBorder(x, z))
                    {
                        continue;
                    }

                    int height = GetHigestSurrounding(x, z, chunk, level);
                    if (height == 0)
                    {
                        continue;
                    }

                    //var skyLight = chunk.GetSkyLight(x, height, z);
                    //if (skyLight == 15)
                    {
                        Block block = level.GetConvertedBlock(new BlockCoordinates(x + (chunk.x * 16), height, z + (chunk.z * 16)));
                        Calculate(level, block);
                    }
                    //else
                    //{
                    //	Log.Error($"Block with wrong light level. Expected 15 but was {skyLight}");
                    //}
                }
            }

            return true;
        }

        private void Calculate(World level, Block block)
        {
            try
            {
                if (block.SkyLight != 15)
                {
                    Log.Error($"Block at {block.Coordinates} had unexpected light level. Expected 15 but was {block.SkyLight}");
                }

                var lightBfsQueue = new Queue<BlockCoordinates>();

                /*if (!lightBfsQueue.Contains(block.Coordinates)) */
                lightBfsQueue.Enqueue(block.Coordinates);
                while (lightBfsQueue.Count > 0)
                {
                    ProcessNode(level, lightBfsQueue.Dequeue(), lightBfsQueue);
                }
            }
            catch (Exception e)
            {
                Log.Error("Calculation", e);
            }
        }

        private void ProcessNode(World level, BlockCoordinates coordinates, Queue<BlockCoordinates> lightBfsQueue)
        {
            byte currentSkyLight = level.GetSkyLight(coordinates);

            byte maxSkyLight = currentSkyLight;
            if (coordinates.Y < 256)
            {
                var up = coordinates + BlockCoordinates.Up;
                maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, up, currentSkyLight, up: true));
            }

            if (coordinates.Y > 0)
            {
                var down = coordinates + BlockCoordinates.Down;
                maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, down, currentSkyLight, down: true));
            }

            var west = coordinates + BlockCoordinates.West;
            maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, west, currentSkyLight));


            var east = coordinates + BlockCoordinates.East;
            maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, east, currentSkyLight));


            var south = coordinates + BlockCoordinates.South;
            maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, south, currentSkyLight));

            var north = coordinates + BlockCoordinates.North;
            maxSkyLight = Math.Max(maxSkyLight, SetLightLevel(level, lightBfsQueue, north, currentSkyLight));

            if (level.IsAir(coordinates) && currentSkyLight != 15)
            {
                maxSkyLight = (byte)Math.Max(currentSkyLight, maxSkyLight - 1);

                if (maxSkyLight > currentSkyLight)
                {
                    level.SetSkyLight(coordinates, maxSkyLight);

                    /*if (!lightBfsQueue.Contains(coordinates)) */
                    lightBfsQueue.Enqueue(coordinates);
                }
            }
        }

        private byte SetLightLevel(World level, Queue<BlockCoordinates> lightBfsQueue, BlockCoordinates coordinates, byte lightLevel, bool down = false, bool up = false)
        {
            if (!down && !up && coordinates.Y >= level.GetHeight(coordinates))
            {
                level.SetSkyLight(coordinates, 15);
                return 15;
            }

            bool isTransparent = level.IsTransparent(coordinates);
            byte skyLight = level.GetSkyLight(coordinates);

            if (down && isTransparent && lightLevel == 15)
            {
                if (skyLight != 15)
                {
                    level.SetSkyLight(coordinates, 15);
                }

                /*if (!lightBfsQueue.Contains(coordinates)) */
                lightBfsQueue.Enqueue(coordinates);

                return 15;
            }

            if (isTransparent && skyLight + 2 <= lightLevel)
            {
                byte newLevel = (byte)(lightLevel - 1);
                level.SetSkyLight(coordinates, newLevel);

                /*if (!lightBfsQueue.Contains(coordinates)) */
                lightBfsQueue.Enqueue(coordinates);
                return newLevel;
            }

            return skyLight;
        }
        
        public static void CheckIfSpawnIsMiddle(IOrderedEnumerable<ChunkColumn> chunks, Vector3 spawnPoint)
        {
            int xMin = chunks.OrderBy(kvp => kvp.x).First().x;
            int xMax = chunks.OrderByDescending(kvp => kvp.x).First().x;
            int xd = Math.Abs(xMax - xMin);

            int zMin = chunks.OrderBy(kvp => kvp.z).First().z;
            int zMax = chunks.OrderByDescending(kvp => kvp.z).First().z;
            int zd = Math.Abs(zMax - zMin);

            int xm = (int)((xd / 2f) + xMin);
            int zm = (int)((zd / 2f) + zMin);

            if (xm != (int)spawnPoint.X >> 4) Log.Warn($"Wrong spawn X={xm}, {(int)spawnPoint.X >> 4}");
            if (zm != (int)spawnPoint.Z >> 4) Log.Warn($"Wrong spawn Z={zm}, {(int)spawnPoint.Z >> 4}");

            if (zm == (int)spawnPoint.Z >> 4 && xm == (int)spawnPoint.X >> 4) Log.Warn($"Spawn correct {xm}, {zm} and {(int)spawnPoint.X >> 4}, {(int)spawnPoint.Z >> 4}");
        }
        
        private static int _chunkCount;
        

        private static bool IsOnChunkBorder(int x, int z)
        {
            return !(x > 0 && x < 15 && z > 0 && z < 15);
        }

        private static int GetHigestSurrounding(int x, int z, ChunkColumn chunk, World level)
        {
            int h = chunk.GetHeight(x, z);
            if (h == 127) return h;

            if (x == 0 || x == 15 || z == 0 || z == 15)
            {
                var coords = new BlockCoordinates(x + (chunk.x * 16), h, z + (chunk.z * 16));

                h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.Up));
                h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.West));
                h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.East));
                h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.North));
                h = Math.Max(h, level.GetHeight(coords + BlockCoordinates.South));
                if (h > 127) h = 127;
                if (h < 0) h = 0;
                return h;
            }

            //if (z < 15) h = Math.Max(h, chunk.GetHeight(x, z + 1));
            //if (z > 0) h = Math.Max(h, chunk.GetHeight(x, z - 1));
            //if (x < 15) h = Math.Max(h, chunk.GetHeight(x + 1, z));
            //if (x < 15 && z > 0) h = Math.Max(h, chunk.GetHeight(x + 1, z - 1));
            //if (x < 15 && z < 15) h = Math.Max(h, chunk.GetHeight(x + 1, z + 1));
            //if (x > 0) h = Math.Max(h, chunk.GetHeight(x - 1, z));
            //if (x > 0 && z > 0) h = Math.Max(h, chunk.GetHeight(x - 1, z - 1));
            //if (x > 0 && z < 15) h = Math.Max(h, chunk.GetHeight(x - 1, z + 1));

            h = Math.Max(h, chunk.GetHeight(x, z + 1));
            h = Math.Max(h, chunk.GetHeight(x, z - 1));
            h = Math.Max(h, chunk.GetHeight(x + 1, z));
            h = Math.Max(h, chunk.GetHeight(x + 1, z - 1));
            h = Math.Max(h, chunk.GetHeight(x + 1, z + 1));
            h = Math.Max(h, chunk.GetHeight(x - 1, z));
            h = Math.Max(h, chunk.GetHeight(x - 1, z - 1));
            h = Math.Max(h, chunk.GetHeight(x - 1, z + 1));

            return h;
        }

        public void ShowHeights(ChunkColumn chunk)
        {
            if (chunk == null) return;

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    for (byte y = 255; y > 0; y--)
                    {
                        if (chunk.GetSkylight(x, y, z) == 0)
                        {
                            chunk.SetBlock(x, y, z, 41);
                        }
                    }
                }
            }
        }
    }
}
