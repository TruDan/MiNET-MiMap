using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BiomeMap.Data;
using BiomeMap.Output;
using BiomeMap.Renderer;
using MiNET.Worlds;

namespace BiomeMap
{
    public class BiomeMapLevelHandler
    {

        protected IChunkRenderer[] Renderers;

        protected IRenderOutput[] Outputs;

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

                foreach (var chunk in chunks)
                {
                    OnChunkUpdate(chunk);
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

        public void OnChunkUpdate(ChunkColumn chunk)
        {
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
