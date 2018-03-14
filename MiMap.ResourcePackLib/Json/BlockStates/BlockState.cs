using System.Collections.Generic;

namespace MiMap.ResourcePackLib.Json.BlockStates
{
	public class BlockState
	{
		/// <summary>
		/// Holds the names of all the variants of the block.
		/// </summary>
		public Dictionary<string, BlockStateVariant> Variants { get; set; } = new Dictionary<string, BlockStateVariant>();

		//public Dictionary<string, >

	}
}
