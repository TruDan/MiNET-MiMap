using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MiMap.Common.Data;
using MiMap.Drawing.Events;
using MiMap.Drawing.Utils;
using Size = MiMap.Common.Data.Size;

namespace MiMap.Drawing
{
    public class TileScaler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TileScaler));

        public event EventHandler<TileUpdateEventArgs> OnTileUpdated;

        private TileScalerRunner[] _runners;

        public string LevelId { get; }
        public string LayerId { get; }

        public TileScaler(string basePath, Size renderScale, Size tileSize, int minZoom, int maxZoom, string levelId, string layerId)
        {
            LevelId = levelId;
            LayerId = layerId;
            var size = maxZoom - minZoom + 1;
            _runners = new TileScalerRunner[size];

            var i = 0;
            for (int z = minZoom; z <= maxZoom; z++)
            {
                _runners[i] = new TileScalerRunner(this, basePath, z, renderScale, tileSize);
                i++;
            }

            //Log.InfoFormat("TileScaler created with id {0} {1}->{2} ({3} - {4}) {5}", layerId, minZoom, maxZoom, tileSize, renderScale, basePath);
        }

        public void Enqueue(RegionPosition regionPos, MapRegionLayer region)
        {
            var entry = new TileScaleEntry(regionPos, region);
            foreach (var runner in _runners)
            {
                runner.Enqueue(entry);
            }
        }

        class TileScaleEntry : IDisposable
        {
            public RegionPosition Position { get; }

            private readonly MapRegionLayer _region;
            private readonly BlockPosition[] _updatedBlocks;

            private Bitmap _bitmap;
            private readonly object _bitmapSync = new object();
	        internal int _updateLock = 0;
            public TileScaleEntry(RegionPosition regionPos, MapRegionLayer region)
            {
                Position = regionPos;
                _region = region;
                _updatedBlocks = region.UpdatedBlocks;
            }

            public Bitmap GetBitmap()
            {
                return _region.GetBitmap();
            }

            public bool IsRegionDirty(BlockBounds bounds)
            {
                foreach (var block in _updatedBlocks)
                {
                    if (bounds.Contains(block))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Dispose()
            {
                //_bitmap?.Dispose();
            }
        }

        class TileScalerRunner
        {
            private static readonly ILog Log = LogManager.GetLogger(typeof(TileScalerRunner));

            public const int ExecInterval = 500;


            public int Zoom { get; }

            protected TileScaler Scaler { get; }

            private int Scale { get; }

            private string BasePath { get; }

            private Size RegionTileSize { get; }

            private Size TileSize { get; }

         //   private readonly ConcurrentQueue<TileScaleEntry> _updates = new ConcurrentQueue<TileScaleEntry>();

        //    private readonly List<TileScaleEntry> _processing = new List<TileScaleEntry>();

      //      private readonly object _queueSync = new object();

      //      private readonly object _taskSync = new object();

	        private Timer _timer;
            public TileScalerRunner(TileScaler baseScaler, string basePath, int zoom, Size renderScale, Size tileSize)
            {
                Scaler = baseScaler;
                BasePath = basePath;
                Zoom = zoom;
                Scale = 1 << Zoom;
                RegionTileSize = new Size((renderScale.Width * (1 << 9)) / Scale, (renderScale.Height * (1 << 9)) / Scale);
                TileSize = tileSize;

			//	_timer = new Timer(TimerCallback, null, 0, ExecInterval);
            }

            public void Enqueue(TileScaleEntry entry)
            {
              //  lock (_queueSync)
                {
                    //if (_updates.(entry)) return;
                   // _updates.Enqueue(entry);
			
					//_updates.Enqueue(entry);
	                DoTask(entry);
                }
            }

            private void TimerCallback(object state)
            {
           //     if (!Monitor.TryEnter(_taskSync, 0))
                {
                    return;
                }

                try
                {
                    DoTask(null);
                }
                finally
                {
           //         Monitor.Exit(_taskSync);
                }
            }

	        private void DoTask(TileScaleEntry e)
	        {
		//         while (_updates.TryDequeue(out TileScaleEntry e))
		        {
			        if (Interlocked.CompareExchange(ref e._updateLock, 1, 0) == 0)
			        {
				        return;
			        }

			        ThreadPool.QueueUserWorkItem((o) =>
			        {
				        try
				        {
					        ScaleRegion(e);
				        }
				        catch (Exception ex)
				        {
					        Log.Error("Exception during region scale " + e.Position, ex);
				        }
				        finally
				        {
					        Interlocked.CompareExchange(ref e._updateLock, 0, 1);
				        }
			        });
		        }
	        }

	        private void ScaleRegion(TileScaleEntry entry)
            {
                var sw = Stopwatch.StartNew();
                var regionPos = entry.Position;

                if (!entry.IsRegionDirty(regionPos.GetBlockBounds()))
                    return;

                using (var baseImg = entry.GetBitmap())
                {
                    var format = baseImg.PixelFormat;
                    var sync = new object();

                    //Parallel.For(0, Scale, tX =>
                    for (int tX = 0; tX < Scale; tX++)
                    {
                        //Parallel.For(0, Scale, tZ =>
                        for (int tZ = 0; tZ < Scale; tZ++)
                        {
                            // Check if update is actually required.
                            var tilePos = new TilePosition((regionPos.X << Zoom) + tX, (regionPos.Z << Zoom) + tZ,
                                Zoom);

                            if (!entry.IsRegionDirty(tilePos.GetBlockBounds()))
                                continue;

                           // lock (sync)
                            {
	                            using (Bitmap img = baseImg.Clone(
		                            new Rectangle(tX * RegionTileSize.Width, tZ * RegionTileSize.Height,
			                            RegionTileSize.Width, RegionTileSize.Height), format))
	                            {
									DrawTile(img, tilePos);
	                            }
                            }


                           // DrawTile(ref img, tilePos);
                            Scaler.OnTileUpdated?.Invoke(this,
                                new TileUpdateEventArgs(Scaler.LevelId, Scaler.LayerId, tilePos));
                        } //);
                    } //);
                }

                //Log.InfoFormat("Scaled Region {0} to Zoom Level {1} in {2}ms", regionPos, Zoom, sw.ElapsedMilliseconds);
            }

	        private void DrawTile(Bitmap baseBitmap, TilePosition tilePos)
	        {
		        using (var tileImg = new Bitmap(TileSize.Width, TileSize.Height))
		        {
			        using (var g = tileImg.GetGraphics())
			        {
				        g.DrawImage(baseBitmap, new Rectangle(0, 0, TileSize.Width, TileSize.Height));
			        }

			        var tilePath = Path.Combine(BasePath, tilePos.Zoom.ToString(),
				        tilePos.X + "_" + tilePos.Z + ".png");

			        tileImg.SaveToFile(tilePath);
			        //Log.InfoFormat("Saved Tile {0} to {1}", tilePos, tilePath);
		        }
	        }
        }
    }
}
