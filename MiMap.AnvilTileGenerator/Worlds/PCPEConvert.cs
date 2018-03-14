using System;
using System.Collections.Generic;
using log4net;
using MiNET.Blocks;

namespace MiMap.AnvilTileGenerator.Worlds
{
    public static class PCPEConvert
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PCPEConvert));
        
        public static List<int> UnconvertedTransparentBlocks { get; private set; }

        public static Dictionary<int, Tuple<int, Func<int, byte, byte>>> ConvertMap => MiNET.Worlds.AnvilWorldProvider.Convert;

        static PCPEConvert()
        {
            UnconvertedTransparentBlocks = GetTransparentBlocks();
        }

        private static List<int> GetTransparentBlocks()
        {
            var baseList = BlockFactory.TransparentBlocks;
            var newList = new List<int>();

            foreach (int blockId in baseList)
            {
                // If its a converted block, convert it back.
                newList.Add(GetOriginalBlockIdFromConvertedId(blockId));
            }
            return newList;
        }

        private static int GetOriginalBlockIdFromConvertedId(int convertedId)
        {
            foreach (KeyValuePair<int, Tuple<int, Func<int, byte, byte>>> convertMap in ConvertMap)
            {
                if (convertMap.Value.Item1.Equals(convertedId))
                {
                    return convertMap.Key;
                }
            }

            return convertedId;
        }

        public static Block ConvertBlock(Block block)
        {
            Block newBlock = ConvertBlock(block.Id, block.Metadata);
            newBlock.Coordinates = block.Coordinates;

            return newBlock;
        }

        public static Block ConvertBlock(int blockId, byte metadata = 0)
        {
            if (ConvertMap.ContainsKey(blockId))
            {
                Func<int, byte, byte> dataConverter = ConvertMap[blockId].Item2;
                blockId = ConvertMap[blockId].Item1;

                metadata = dataConverter(blockId, metadata);
            }

            Block block = BlockFactory.GetBlockById((byte) blockId);
            block.Metadata = metadata;

            return block;
        }
    }
}
