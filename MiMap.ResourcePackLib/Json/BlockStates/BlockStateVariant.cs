using System.Collections.Generic;
using MiMap.ResourcePackLib.Json.Converters;
using Newtonsoft.Json;

namespace MiMap.ResourcePackLib.Json.BlockStates
{
	[JsonConverter(typeof(BlockStateVariantConverter))]
	public class BlockStateVariant : List<BlockStateModel>
	{

	}
}
