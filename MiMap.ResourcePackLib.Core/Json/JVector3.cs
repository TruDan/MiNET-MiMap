using MiMap.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace MiMap.ResourcePackLib.Json
{
	[JsonConverter(typeof(JVector3Converter))]
	public class JVector3
	{
		public static JVector3 Zero => new JVector3(0, 0, 0);

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }

		public JVector3()
		{

		}

		public JVector3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public JVector3(JVector3 v) : this(v.X, v.Y, v.Z)
		{ }

	}
}
