using System.Collections.Generic;
using System.Diagnostics;
using fNbt;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiMap.AnvilTileGenerator.Worlds.Anvil
{
	public class AnvilChunk : ChunkColumn
	{
		public const int Width = 16;

		private List<Block> LightSources { get; set; }

		public AnvilChunk(ChunkCoordinates coords, NbtFile nbtFile) : this(coords)
		{
			LoadFromNbt(nbtFile);
		}

		public AnvilChunk(ChunkCoordinates coords, ChunkColumn chunkColumn) : this(coords)
		{
			CloneFromChunk(chunkColumn);
		}

		private AnvilChunk(ChunkCoordinates coords)
		{
			x = coords.X;
			z = coords.Z;
			LightSources = new List<Block>();
		}

		#region ChunkColumn

		private void CloneFromChunk(ChunkColumn chunkColumn)
		{
			chunks = new Chunk[16];
			for (var i = 0; i < chunks.Length; i++)
			{
				chunks[i] = chunkColumn.chunks[i];
			}

			biomeId = (byte[]) chunkColumn.biomeId.Clone();
			height = (short[]) chunkColumn.height.Clone();

			BlockEntities = new Dictionary<BlockCoordinates, NbtCompound>();
			foreach (KeyValuePair<BlockCoordinates, NbtCompound> blockEntityPair in chunkColumn.BlockEntities)
			{
				BlockEntities.Add(blockEntityPair.Key, (NbtCompound) blockEntityPair.Value.Clone());
			}
		}

		#endregion

		#region Nbt

		private List<NbtTag> Entities = new List<NbtTag>();

		internal NbtFile GetChunkNbt()
		{
			var nbt = new NbtFile();
			nbt.RootTag.Add(new NbtInt("DataVersion", 184));

			var dataTag = new NbtCompound("Level");
			nbt.RootTag.Add(dataTag);

			//dataTag.Add(new NbtLong("InhabitedTime", 0));
			//dataTag.Add(new NbtLong("LastUpdate", 0));
			dataTag.Add(new NbtByte("TerrainPopulated", 1));

			dataTag.Add(new NbtByte("LightPopulated", 0));
			dataTag.Add(new NbtInt("xPos", x));
			dataTag.Add(new NbtInt("zPos", z));
			dataTag.Add(new NbtByteArray("Biomes", biomeId));
			dataTag.Add(new NbtIntArray("HeightMap", new int[256]));

			var sections = new NbtList("Sections");
			dataTag.Add(sections);
			SaveNbtChunkSections(sections);

			var entities = new NbtList("Entities", NbtTagType.Compound);
			dataTag.Add(entities);
			// TODO: Save & load entities
			foreach (NbtTag entityTag in Entities)
			{
				var clone = (NbtTag) entityTag.Clone();
				clone.Name = null;
				entities.Add(clone);
			}

			var blockEntities = new NbtList("TileEntities", NbtTagType.Compound);
			dataTag.Add(blockEntities);
			foreach (NbtCompound blockEntityNbt in BlockEntities.Values)
			{
				var nbtClone = (NbtCompound) blockEntityNbt.Clone();
				nbtClone.Name = null;
				blockEntities.Add(nbtClone);
			}

			dataTag.Add(new NbtList("TileTicks", NbtTagType.Compound));

			return nbt;
		}

		private void SaveNbtChunkSections(NbtList sectionsList)
		{
			for (var i = 0; i < 16; i++)
			{
				var sectionTag = new NbtCompound();
				sectionsList.Add(sectionTag);

				sectionTag.Add(new NbtByte("Y", (byte) i));
				int sy = i * 16;

				var blocks = new byte[4096];
				var data = new byte[2048];
				var blockLight = new byte[2048];
				var skyLight = new byte[2048];

				for (var x = 0; x < 16; x++)
				{
					for (var z = 0; z < 16; z++)
					{
						for (var y = 0; y < 16; y++)
						{
							int yi = sy + y;
							if (yi < 0 || yi >= 256)
							{
								continue; // ?
							}

							int anvilIndex = y * 16 * 16 + z * 16 + x;

							blocks[anvilIndex] = GetBlock(x, yi, z);
							SetNibble4(data, anvilIndex, GetMetadata(x, yi, z));
							SetNibble4(blockLight, anvilIndex, GetBlocklight(x, yi, z));
							SetNibble4(skyLight, anvilIndex, GetSkylight(x, yi, z));
						}
					}
				}

				sectionTag.Add(new NbtByteArray("Blocks", blocks));
				sectionTag.Add(new NbtByteArray("Data", data));
				sectionTag.Add(new NbtByteArray("BlockLight", blockLight));
				sectionTag.Add(new NbtByteArray("SkyLight", skyLight));
			}
		}


		private void LoadFromNbt(NbtFile nbtFile)
		{
			NbtTag dataTag = nbtFile.RootTag["Level"];
			//Debug.WriteLine($"Chunk {x},{z}: {nbtFile}");

			biomeId = dataTag["Biomes"].ByteArrayValue;
			isAllAir = true;

			var sections = dataTag["Sections"] as NbtList;
			foreach (NbtTag sectionTag in sections)
			{
				LoadNbtSection(sectionTag);
			}

			var entities = dataTag["Entities"] as NbtList;
			if (entities != null)
			{
				foreach (NbtTag entityTag in entities)
				{
					LoadNbtEntity(entityTag);
				}
			}

			var blockEntities = dataTag["TileEntities"] as NbtList;
			if (blockEntities != null)
			{
				foreach (NbtTag blockEntityTag in blockEntities)
				{
					LoadNbtBlockEntity(blockEntityTag);
				}
			}

			isDirty = false;
		}

		private void LoadNbtSection(NbtTag sectionTag)
		{
			int sy = sectionTag["Y"].ByteValue * 16;
			byte[] blocks = sectionTag["Blocks"].ByteArrayValue;
			byte[] data = sectionTag["Data"].ByteArrayValue;

			NbtTag addTag = sectionTag["Add"];
			var adddata = new byte[2048];
			if (addTag != null)
			{
				adddata = addTag.ByteArrayValue;
			}
			byte[] blockLight = sectionTag["BlockLight"].ByteArrayValue;
			byte[] skyLight = sectionTag["SkyLight"].ByteArrayValue;

			for (var x = 0; x < 16; x++)
			{
				for (var z = 0; z < 16; z++)
				{
					for (var y = 0; y < 16; y++)
					{
						int yi = sy + y;
						if (yi < 0 || yi > 256)
						{
							continue;
						}

						int anvilIndex = y * 16 * 16 + z * 16 + x;
						byte blockId_a = blocks[anvilIndex];
						byte blockId_b = GetNibble4(adddata, anvilIndex);
						int blockId = blockId_a + (blockId_b << 8);

						isAllAir = isAllAir && blockId == 0;
						byte metadata = GetNibble4(data, anvilIndex);

						SetBlock(x, yi, z, (byte) blockId);
						SetMetadata(x, yi, z, metadata);
						SetBlocklight(x, yi, z, GetNibble4(blockLight, anvilIndex));
						SetSkyLight(x, yi, z, GetNibble4(skyLight, anvilIndex));

						Block convertedBlock = PCPEConvert.ConvertBlock(blockId, metadata);

						if (convertedBlock.LightLevel > 0)
						{
							convertedBlock.Coordinates = new BlockCoordinates(x + (16 * this.x), yi, z + (16 * this.z));
							LightSources.Add(convertedBlock);
						}
					}
				}
			}
		}

		private void LoadNbtEntity(NbtTag entityTag)
		{
			Entities.Add(entityTag);
		}

		private void LoadNbtBlockEntity(NbtTag nbtTag)
		{
			var blockEntityTag = (NbtCompound) nbtTag.Clone();
			int x = blockEntityTag["x"].IntValue;
			int y = blockEntityTag["y"].IntValue;
			int z = blockEntityTag["z"].IntValue;

			var coords = new BlockCoordinates(x, y, z);
			Debug.WriteLine($"Found TileEntity at {x},{y},{z}: {nbtTag["id"].StringValue}");

			BlockEntities.Add(coords, blockEntityTag);
		}

		private static byte GetNibble4(byte[] arr, int index)
		{
			return (byte) (index % 2 == 0 ? arr[index / 2] & 0x0F : (arr[index / 2] >> 4) & 0x0F);
		}

		private static void SetNibble4(byte[] arr, int index, byte value)
		{
			if (index % 2 == 0)
			{
				arr[index / 2] = (byte) ((value & 0x0F) | arr[index / 2]);
			}
			else
			{
				arr[index / 2] = (byte) (((value << 4) & 0xF0) | arr[index / 2]);
			}
		}

		#endregion
	}
}
