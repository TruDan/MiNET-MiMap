using System;
using System.Collections.Generic;
using System.Diagnostics;
using fNbt;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using AnvilWorldProvider = MiMap.AnvilTileGenerator.Worlds.Anvil.AnvilWorldProvider;

namespace MiMap.AnvilTileGenerator.Worlds
{
	public class World
	{
		public AnvilWorldProvider WorldProvider { get; }

		public string Directory => WorldProvider.BasePath;

		public string Name { get; }


		internal World(string worldPath, string name)
		{
			WorldProvider = new AnvilWorldProvider(worldPath);
			Name = name;
		}

		public void Initialise()
		{
			WorldProvider.Initialize();
		}

		public void Load()
		{
			Initialise();
			GenerateChunkRadius(1);
		}

		public void LoadAllChunks()
		{
			WorldProvider.LoadAllChunks();
		}

		public void Optimise()
		{
			WorldProvider.PruneAir();
			WorldProvider.MakeAirChunksAroundWorldToCompensateForBadRendering();
		}

		public void Save()
		{
			WorldProvider.SaveChunks();
		}
		
		public void SetBlock(Block block)
		{
			ChunkColumn chunk = GetChunk(block.Coordinates);

			chunk.SetBlock(block.Coordinates.X & 0x0f, block.Coordinates.Y & 0xff, block.Coordinates.Z & 0x0f, block.Id);
			chunk.SetMetadata(block.Coordinates.X & 0x0f, block.Coordinates.Y & 0xff, block.Coordinates.Z & 0x0f, block.Metadata);
		}
		
		public Block GetConvertedBlock(BlockCoordinates blockCoordinates)
		{
			ChunkColumn chunk = GetChunk(blockCoordinates);
			if (chunk == null) return new Air() { Coordinates = blockCoordinates, SkyLight = 15 };

			byte bid = chunk.GetBlock(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			byte metadata = chunk.GetMetadata(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			byte blockLight = chunk.GetBlocklight(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			byte skyLight = chunk.GetSkylight(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);



			Block block = PCPEConvert.ConvertBlock(bid, metadata);

			block.Coordinates = blockCoordinates;
			block.BlockLight = blockLight;
			block.SkyLight = skyLight;

			return block;
		}

		public void SetBlockLight(Block block)
		{
			ChunkColumn chunk = GetChunk(block.Coordinates);
			chunk.SetBlocklight(block.Coordinates.X & 0x0f, block.Coordinates.Y & 0xff, block.Coordinates.Z & 0x0f, block.BlockLight);
		}

		public void SetSkyLight(Block block)
		{
			ChunkColumn chunk = GetChunk(block.Coordinates);
			chunk.SetSkyLight(block.Coordinates.X & 0x0f, block.Coordinates.Y & 0xff, block.Coordinates.Z & 0x0f, block.SkyLight);
		}

		public void SetSkyLight(BlockCoordinates coordinates, byte skyLight)
		{
			ChunkColumn chunk = GetChunk(coordinates);
			chunk?.SetSkyLight(coordinates.X & 0x0f, coordinates.Y & 0xff, coordinates.Z & 0x0f, skyLight);
		}

		public bool IsAir(BlockCoordinates blockCoordinates)
		{
			ChunkColumn chunk = GetChunk(blockCoordinates);
			if (chunk == null) return true;

			byte bid = chunk.GetBlock(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			return bid == 0;
		}

		public bool IsTransparent(BlockCoordinates blockCoordinates)
		{
			ChunkColumn chunk = GetChunk(blockCoordinates);
			if (chunk == null) return true;

			byte bid = chunk.GetBlock(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0xff, blockCoordinates.Z & 0x0f);
			return PCPEConvert.UnconvertedTransparentBlocks.Contains(bid);
		}

		public int GetHeight(BlockCoordinates blockCoordinates)
		{
			ChunkColumn chunk = GetChunk(blockCoordinates);
			if (chunk == null) return 256;

			return chunk.GetHeight(blockCoordinates.X & 0x0f, blockCoordinates.Z & 0x0f);
		}

		public byte GetSkyLight(BlockCoordinates blockCoordinates)
		{
			ChunkColumn chunk = GetChunk(blockCoordinates);

			if (chunk == null) return 15;

			return chunk.GetSkylight(blockCoordinates.X & 0x0f, blockCoordinates.Y & 0x7f, blockCoordinates.Z & 0x0f);
		}

		private ChunkColumn GetChunk(BlockCoordinates blockCoordinates)
		{
			return WorldProvider.GenerateChunkColumn(new ChunkCoordinates(blockCoordinates.X >> 4, blockCoordinates.Z >> 4));
		}

		private ChunkColumn GetChunk(int x, int z)
		{
			return WorldProvider.GenerateChunkColumn(new ChunkCoordinates(x, z));
		}

		public ChunkColumn[] GetLoadedChunks()
		{
			return WorldProvider.GetAllLoadedChunks();
		}

		public void RecalculateLighting()
		{
			RecalculateSkyLight();
			RecalculateBlockLight();
		}

		public void RecalculateSkyLight()
		{
			Lighting.SkyLightCalculations.Calculate(this);
		}

		public void RecalculateBlockLight()
		{
			Queue<Block> lightSources = WorldProvider.GetLightSources();
			while (lightSources.Count > 0)
			{
				Block block = lightSources.Dequeue();
				block = GetConvertedBlock(block.Coordinates);
				Lighting.BlockLightCalculations.Calculate(this, block);
			}
		}

		public void GenerateChunkRadius(int radius)
		{
			for (int x = radius; x > -radius; x--)
			{
				for (int z = radius; z > -radius; z--)
				{
					//Debug.WriteLine($"Generating Chunk at ({x},{z})");
					WorldProvider.GenerateChunkColumn(new ChunkCoordinates(x, z));
				}
			}
		}
	}
}
