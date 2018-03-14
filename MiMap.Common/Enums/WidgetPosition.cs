using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MiMap.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WidgetPosition
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }
}
