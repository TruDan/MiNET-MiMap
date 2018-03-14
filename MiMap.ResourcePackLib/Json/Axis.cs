using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.ResourcePackLib.Json
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum Axis
	{
		Undefined,
		X,
		Y,
		Z
	}
}
