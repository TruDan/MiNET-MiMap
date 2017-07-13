using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiomeMap.Output;
using Fleck;
using MiNET;
using MiNET.Net;

namespace BiomeMap.Http
{
    public class BiomeMapSocketServer
    {

        private static readonly object _sync = new object();
        public static List<IWebSocketConnection> Connections = new List<IWebSocketConnection>();

        private static Dictionary<Player, DateTime> _knownPlayers = new Dictionary<Player, DateTime>();


        public static void Broadcast(string message)
        {
            lock (_sync)
            {
                foreach (var c in Connections)
                {
                    c.Send(message);
                }
            }
        }

        public static void OnOpen(IWebSocketConnection c)
        {
            lock (_sync)
            {
                Connections.Add(c);

                foreach (var p in _knownPlayers.Keys.ToArray())
                {
                    c.Send(string.Format("player:join:{0}:{1}", p.ClientUuid.ToString(), p.KnownPosition.X + ",") +
                           p.KnownPosition.Z);
                }
            }
        }

        public static void OnClose(IWebSocketConnection c)
        {
            lock (_sync)
            {
                Connections.Remove(c);
            }
        }

        public static void OnMessage(IWebSocketConnection c, string message)
        {
            
        }

        public static void OnPlayerJoin(Player player)
        {
            _knownPlayers.Add(player, DateTime.UtcNow);
            Broadcast(string.Format("player:join:{0}:{1}", player.ClientUuid.ToString(), player.KnownPosition.X + "," + player.KnownPosition.Z));


            var bytes = player.Skin.Texture;
            int width = 64;
            var height = bytes.Length == 64 * 32 * 4 ? 32 : 64;

            using (var skin = new Bitmap(width, height))
            {

                int i = 0;
                for (int y = 0; y < skin.Height; y++)
                {
                    for (int x = 0; x < skin.Width; x++)
                    {
                        byte r = bytes[i++];
                        byte g = bytes[i++];
                        byte b = bytes[i++];
                        byte a = bytes[i++];

                        Color color = Color.FromArgb(a, r, g, b);
                        skin.SetPixel(x, y, color);
                    }
                }


                using (var head = new Bitmap(38, 38))
                {
                    using (var g = Graphics.FromImage(head))
                    {
                        int radius = 3;

                        int diameter = radius * 2;
                        Size size = new Size(diameter, diameter);

                        Rectangle bounds = new Rectangle(2, 2, head.Width-4, head.Height-4);

                        Rectangle arc = new Rectangle(bounds.Location, size);
                        using (GraphicsPath path = new GraphicsPath())
                        {

                            if (radius == 0)
                            {
                                path.AddRectangle(bounds);
                            }
                            else
                            {
                                // top left arc  
                                path.AddArc(arc, 180, 90);

                                // top right arc  
                                arc.X = bounds.Right - diameter;
                                path.AddArc(arc, 270, 90);

                                // bottom right arc  
                                arc.Y = bounds.Bottom - diameter;
                                path.AddArc(arc, 0, 90);

                                // bottom left arc 
                                arc.X = bounds.Left;
                                path.AddArc(arc, 90, 90);

                                path.CloseFigure();
                            }

                            g.Clip = new Region(path);

                            g.InterpolationMode = InterpolationMode.NearestNeighbor;
                            g.DrawImage(skin,
                                new Rectangle(0, 0, head.Width, head.Height),
                                new Rectangle(8, 8, 8, 8),GraphicsUnit.Pixel);
                            g.ResetClip();

                            g.DrawPath(new Pen(Color.DarkSlateGray, 2), path);
                        }
                    }

                    head.Save(Path.Combine(BitmapOutput.OutputPath, "PlayerHead", player.ClientUuid + ".png"));
                }
            }

        }

        public static void OnPlayerQuit(Player player)
        {
            Broadcast(string.Format("player:quit:{0}", player.ClientUuid.ToString()));
        }

        public static void OnPlayerMove(Player player)
        {
            DateTime updatedTime;
            if (_knownPlayers.TryGetValue(player, out updatedTime))
            {
                if (DateTime.UtcNow - updatedTime > TimeSpan.FromMilliseconds(500))
                {
                    Broadcast(string.Format("player:move:{0}:{1}", player.ClientUuid.ToString(),
                        player.KnownPosition.X + "," + player.KnownPosition.Z));
                    _knownPlayers[player] = DateTime.UtcNow;
                }
            }
        }
    }
}
