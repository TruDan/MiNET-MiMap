using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using log4net;

namespace BiomeMap.Output
{
        public class TileBitmap
        {
            private static readonly ILog Log = LogManager.GetLogger(typeof(TileBitmap));
            public Bitmap Bitmap { get; }
            public string FilePath { get; }

            public int ZoomLevel { get; }

            public int TileX { get; }
            public int TileY { get; }

            public Rectangle ChunkBounds { get; }

            public bool IsDirty { get; private set; }

            private static readonly ImageCodecInfo Encoder;
            private static readonly EncoderParameters EncoderParameters;

            static TileBitmap()
            {
                Encoder = GetEncoder(ImageFormat.Jpeg);

                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 75L);
                EncoderParameters.Param[0] = myEncoderParameter;
            }

            public TileBitmap(Bitmap bitmap, string level, int tileX, int tileY, int zoomLevel)
            {
                Bitmap = bitmap;
                TileX = tileX;
                TileY = tileY;
                ZoomLevel = zoomLevel;
                

                //ChunkBounds = new Rectangle((tileX * RegionSize) >> zoomLevel, (tileY * RegionSize) >> zoomLevel, RegionSize >> zoomLevel, RegionSize >> zoomLevel);

                //FilePath = Path.Combine(TilesPath, level, zoomLevel.ToString(), tileX.ToString(), tileY + ".jpg");
                //Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                //Log.InfoFormat("Tile Created {2} @ {0},{1} - Region {3},{4}", TileX, TileY, ZoomLevel);
            }

            public Graphics CreateGraphics()
            {
                //if(!ChunkBounds.Contains(chunkX, chunkZ) && chunkX != ChunkBounds.X && chunkX != ChunkBounds.X+ChunkBounds.Width && chunkZ != )
               //     throw new InvalidDataException($"{chunkX},{chunkZ} is not within tile bounds {ChunkBounds}");

                var g = Graphics.FromImage(Bitmap);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighQuality;
                
               // var s = (TileSize/16f) / ChunkBounds.Width*16f;
                //g.ScaleTransform((ChunkBounds.Width/((float)TileSize/RegionSize)) * (1 << ZoomLevel), (ChunkBounds.Height / ((float)TileSize / RegionSize)) * (1 << ZoomLevel));

                IsDirty = true;
                return g;
            }

            public bool Save()
            {
                if (!IsDirty) return false;

                using (var ms = new MemoryStream())
                {
                    Bitmap.Save(ms, Encoder, EncoderParameters);

                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        byte[] bytes = (byte[]) state;

                        File.WriteAllBytes(FilePath, bytes);
                    }, ms.ToArray());
                }

                return true;
            }
            private static ImageCodecInfo GetEncoder(ImageFormat format)
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                foreach (ImageCodecInfo codec in codecs)
                {
                    if (codec.FormatID == format.Guid)
                    {
                        return codec;
                    }
                }
                return null;
            }

        }
    
}
