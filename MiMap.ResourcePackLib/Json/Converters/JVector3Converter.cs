using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiMap.ResourcePackLib.Json.Converters
{

	public class JVector3Converter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var v = value as JVector3;

			serializer.Serialize(writer, new int[]
			{
				v.X,
				v.Y,
				v.Z
			});
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var obj = JToken.Load(reader);

			if (obj.Type == JTokenType.Array)
			{
				var arr = (JArray)obj;
				if (arr.Count == 3 && arr.All(token => token.Type == JTokenType.Integer))
				{
					return new JVector3()
					{
						X = arr[0].Value<int>(),
						Y = arr[1].Value<int>(),
						Z = arr[2].Value<int>()
					};
				}
			}

			return null;
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(JVector3).IsAssignableFrom(objectType);
		}
	}
}
