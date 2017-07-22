using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Utils;
using MiNET.Worlds;

namespace BiomeMap.GeneratorPlugin
{
    [Plugin(PluginName = "BiomeMap.Generator")]
    public class BiomeMapGeneratorPlugin : Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BiomeMapGeneratorPlugin));

        public static int ChunkRadius = 128;
        public static int ChunkSaveInterval = 64;

        protected override void OnEnable()
        {
            base.OnEnable();

            ChunkRadius = Config.GetProperty("GenChunks", 128);
            Context.LevelManager.LevelCreated += LevelManagerOnLevelCreated;
        }

        private void LevelManagerOnLevelCreated(object sender, LevelEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                var level = e.Level;

                var maxPoints = ChunkRadius * ChunkRadius;

                int di = 1;
                int dj = 0;
                int segment_length = 1;

                int i = 0;
                int j = 0;
                int segment_passed = 0;

                for (int k = 0; k < maxPoints; ++k)
                {
                    i += di;
                    j += dj;
                    ++segment_passed;

                    {
                        var coords = new ChunkCoordinates(i, j);
                        //ThreadPool.QueueUserWorkItem(c => level.GetChunk((ChunkCoordinates) c), coords);

                        level.GetChunk(coords);

                        if (k % ChunkSaveInterval == 0)
                        {
                            level.WorldProvider.SaveChunks();
                            Log.InfoFormat("Saving Level... {2:F1}% ({0}/{1})", k, maxPoints, (k/(float)maxPoints)*100);
                        }

                    }

                    if (segment_passed == segment_length)
                    {
                        segment_passed = 0;

                        int buffer = di;
                        di -= dj;
                        dj = buffer;

                        if (dj == 0)
                        {
                            ++segment_length;
                        }
                    }
                }
            });
        }
    }
}
