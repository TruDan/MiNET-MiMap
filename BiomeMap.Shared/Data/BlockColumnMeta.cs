using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiomeMap.Shared.Data
{
    public class BlockColumnMeta
    {

        public BlockPosition Position { get; set; }

        public int BlockId { get; set; }

        public byte Height { get; set; }

        public byte BiomeId { get; set; }

    }
}
