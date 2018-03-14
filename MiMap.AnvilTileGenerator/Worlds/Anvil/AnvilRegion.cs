using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using fNbt;
using MiNET.Utils;
using MiNET.Worlds;
using AnvilWorldProvider = MiMap.AnvilTileGenerator.Worlds.Anvil.AnvilWorldProvider;

namespace MiMap.AnvilTileGenerator.Worlds.Anvil
{
	public class AnvilRegion
	{
		private AnvilWorldProvider Provider { get; }
		public int X { get; }

		public int Z { get; }

		public const int Width = 32;

		public bool IsLoaded { get; private set; } = false;

		private ConcurrentDictionary<ChunkCoordinates, NbtFile> _containingChunks =
			new ConcurrentDictionary<ChunkCoordinates, NbtFile>();

		private ConcurrentDictionary<ChunkCoordinates, AnvilChunk> _chunkCache =
			new ConcurrentDictionary<ChunkCoordinates, AnvilChunk>();

		private object _cacheSync = new object();

		internal AnvilRegion(AnvilWorldProvider provider, int x, int z)
		{
			Provider = provider;
			X = x;
			Z = z;
		}

		private AnvilChunk GenerateChunk(ChunkCoordinates coords)
		{
			lock (_cacheSync)
			{
				NbtFile chunkNbt;
				if (_containingChunks.TryGetValue(coords, out chunkNbt))
				{
					//Debug.WriteLine($"Generating Chunk ({coords.X}, {coords.Z}) from Nbt");
					var chunk = new AnvilChunk(coords, chunkNbt);
					_chunkCache[coords] = chunk;
					return chunk;
				}
				else
				{
					return null;
				}
			}
		}

		internal AnvilChunk GetChunk(ChunkCoordinates coords, bool generate = true)
		{
			lock (_cacheSync)
			{
				AnvilChunk chunk;
				if (_chunkCache.TryGetValue(coords, out chunk))
				{
					return chunk;
				}

				if (generate)
				{
					return GenerateChunk(coords);
				}
				return null;
			}
		}

		internal void DeleteChunk(ChunkCoordinates coords)
		{
			lock (_cacheSync)
			{
				AnvilChunk chunk;
				if (_chunkCache.TryRemove(coords, out chunk)) {}

				NbtFile nbtFile;
				if (_containingChunks.TryRemove(coords, out nbtFile)) {}
			}
		}

		internal AnvilChunk[] GetLoadedChunks()
		{
			lock (_cacheSync)
			{
				return _chunkCache.Values.ToArray();
			}
		}

		internal void LoadAllChunks()
		{
			lock (_cacheSync)
			{
				foreach (ChunkCoordinates coords in _containingChunks.Keys)
				{
					if (_chunkCache.ContainsKey(coords))
					{
						continue;
					}

					GenerateChunk(coords);
				}
			}
		}


		private AnvilRegionHeader RegionHeader;

		private string FileName => $"r.{X}.{Z}.mca";
		private string FilePath => Path.Combine(Provider.BasePath, "region", FileName);

		public void Load()
		{
			lock (_chunkCache)
			{
				string filePath = FilePath;
				if (!File.Exists(filePath))
				{
					// Welp, nothing to load.
					IsLoaded = true;
					return;
				}

				using (FileStream regionFile = File.OpenRead(filePath))
				{
					AnvilRegionHeader header = LoadRegionHeader(regionFile);
					RegionHeader = header;

					//File.WriteAllLines(FilePath + ".header", header.GetAllChunkData().Select(h => string.Format("{0,-10}\t=>\t{1,-10}\t{2,-10}\t{3,-10}", h.ChunkCoordinates, h.LocationOffset, h.SectorCount, h.UpdatedTimestamp)).ToArray());

					foreach (AnvilRegionHeaderChunkData data in header.GetAllChunkData())
					{
						// Attempt to load NbtFile for each chunk.
						if (!data.ChunkExists)
						{
							continue;
						}

						regionFile.Seek(data.LocationOffset, SeekOrigin.Begin);
						var lengthBuffer = new byte[4];
						regionFile.Read(lengthBuffer, 0, 4);
						Array.Reverse(lengthBuffer);

						int nbtDataLength = BitConverter.ToInt32(lengthBuffer, 0) - 1;


						int compressionMode = regionFile.ReadByte();
						NbtCompression compression = compressionMode == 2
														? NbtCompression.ZLib
														: (compressionMode == 1 ? NbtCompression.GZip : NbtCompression.None);

						var nbtDataBuffer = new byte[nbtDataLength];
						regionFile.Read(nbtDataBuffer, 0, nbtDataLength);

						//Debug.WriteLine($"({data.ChunkCoordinates.X}, {data.ChunkCoordinates.Z}) Offset: {data.LocationOffset}, Header Length: {data.SectorCount}, NbtDataLength: {nbtDataLength}");

						var nbtFile = new NbtFile();
						nbtFile.LoadFromBuffer(nbtDataBuffer, 0, nbtDataLength, compression);

						//File.WriteAllText(Path.Combine(Provider.BasePath, "region", FileName + $"_{data.ChunkCoordinates.X}-{data.ChunkCoordinates.Z}.txt"), nbtFile.ToString());

						//Debug.WriteLine(
						//	$"Region: ({X},{Z}), Chunk({data.ChunkCoordinates.X}, {data.ChunkCoordinates.Z}): NbtDataLength: {nbtDataLength}");

						_containingChunks.TryAdd(data.ChunkCoordinates, nbtFile);
					}
				}

				IsLoaded = true;
			}
		}

		public int Save()
		{
			int count = 0;
			lock (_chunkCache)
			{
				// Save region header
				string filePath = FilePath + ".tmp";
				using (FileStream regionFile = File.OpenWrite(filePath))
				{
					var _chunkNbtBytes = new Dictionary<ChunkCoordinates, byte[]>();

					foreach (KeyValuePair<ChunkCoordinates, AnvilChunk> kvp in _chunkCache)
					{
						byte[] nbtBuffer = kvp.Value.GetChunkNbt().SaveToBuffer(NbtCompression.ZLib);
						_chunkNbtBytes.Add(kvp.Key, nbtBuffer);
						//Debug.WriteLine($"Getting ChunkNbt for: ({kvp.Key.X}, {kvp.Key.Z}) Size: {nbtBuffer.Length}");
					}

					// Add unloaded chunks.
					foreach (KeyValuePair<ChunkCoordinates, NbtFile> kvp in _containingChunks)
					{
						if (!_chunkNbtBytes.ContainsKey(kvp.Key))
						{
							byte[] nbtBuffer = kvp.Value.SaveToBuffer(NbtCompression.ZLib);
							_chunkNbtBytes.Add(kvp.Key, nbtBuffer);
							//Debug.WriteLine($"Loading Existing ChunkNbt for: ({kvp.Key.X}, {kvp.Key.Z}) Size: {nbtBuffer.Length}");
						}
					}

					// Skip header stuff, lets write the chunks then write the headers after.
					regionFile.Seek(8192, SeekOrigin.Begin);

					var chunkOffsetAndLength = new byte[4096];
					var chunkLastUpdated = new byte[4096];

					foreach (KeyValuePair<ChunkCoordinates, byte[]> kvp in _chunkNbtBytes)
					{
						var offset = (int) regionFile.Position;
						int length = kvp.Value.Length + 1;


						int rx = kvp.Key.X % Width;
						if (rx < 0)
						{
							rx += Width;
						}
						int rz = kvp.Key.Z % Width;
						if (rz < 0)
						{
							rz += Width;
						}

						//Debug.WriteLine($"Saving Chunk: ({kvp.Key.X}, {kvp.Key.Z}) [{rx},{rz}] at Offset {offset} with Length: {length}");

						byte[] chunkLengthBytes = BitConverter.GetBytes(length);
						Array.Reverse(chunkLengthBytes);
						regionFile.Write(chunkLengthBytes, 0, 4);

						regionFile.WriteByte(0x02); // Compression: ZLib

						int index = (rx + rz * Width) * 4;

						var chunkSectors = (byte) Math.Ceiling(length / 4096d);


						byte[] chunkOffsetBytes = BitConverter.GetBytes(offset >> 4);
						Array.Reverse(chunkOffsetBytes);
						Array.Copy(chunkOffsetBytes, 0, chunkOffsetAndLength, index, 3);
						chunkOffsetAndLength[index + 3] = chunkSectors;

						byte[] updateTimeBytes = BitConverter.GetBytes(666); // LastUpdatedTime = 666;
						Array.Reverse(updateTimeBytes);
						Array.Copy(updateTimeBytes, 0, chunkLastUpdated, index, 4);

						regionFile.Write(kvp.Value, 0, kvp.Value.Length);

						// 4 = Prefixed Length int
						int remainder;
						Math.DivRem((int) regionFile.Position, 4096, out remainder);

						var padding = new byte[4096 - remainder];
						if (padding.Length > 0)
						{
							regionFile.Write(padding, 0, padding.Length);
						}

						count++;
					}

					regionFile.Seek(0, SeekOrigin.Begin);

					// Write Headers
					regionFile.Write(chunkOffsetAndLength, 0, 4096);

					// Write Headers
					regionFile.Write(chunkLastUpdated, 0, 4096);
				}

				if (File.Exists(FilePath))
				{
					File.Delete(FilePath);
				}

				File.Move(filePath, FilePath);

			}
			return count;
		}


		private AnvilRegionHeader LoadRegionHeader(FileStream regionFile)
		{
			var header = new AnvilRegionHeader();

			var completeUpdatedTimeBuffer = new byte[4096];
			regionFile.Seek(4096, SeekOrigin.Begin);
			regionFile.Read(completeUpdatedTimeBuffer, 0, 4096);

			regionFile.Seek(0, SeekOrigin.Begin);

			//int read = 0;
			int baseX = X * Width;
			int baseZ = Z * Width;

			// Load Chunk Locations
			for (var zi = 0; zi < Width; zi++)
			{
				for (var xi = 0; xi < Width; xi++)
				{
					int x = baseX + xi;
					int z = baseZ + zi;

					int index = (xi + zi * Width) * 4;

					var coords = new ChunkCoordinates(x, z);

					var chunkLocationBuffer = new byte[4];
					regionFile.Read(chunkLocationBuffer, 0, 3);
					Array.Reverse(chunkLocationBuffer);
					int offset = BitConverter.ToInt32(chunkLocationBuffer, 0) << 4;
					int length = regionFile.ReadByte() << 12;

					var updatedTimeBuffer = new byte[4];
					Array.Copy(completeUpdatedTimeBuffer, index, updatedTimeBuffer, 0, 4);

					Array.Reverse(updatedTimeBuffer);
					int updatedTime = BitConverter.ToInt32(updatedTimeBuffer, 0);

					//Debug.WriteLine($"Header ({x},{z}) Offset: {offset}	Length: {length} UpdatedTime: {updatedTime}");
					var chunkHeader = new AnvilRegionHeaderChunkData(coords, offset, length, updatedTime);
					header.SetChunkData(coords, chunkHeader);
				}
			}
			return header;
		}
	}

	class AnvilRegionHeader
	{
		private Dictionary<ChunkCoordinates, AnvilRegionHeaderChunkData> _chunkLocations =
			new Dictionary<ChunkCoordinates, AnvilRegionHeaderChunkData>();

		internal void SetChunkData(ChunkCoordinates coords, AnvilRegionHeaderChunkData data)
		{
			//if (_chunkLocations.ContainsKey(coords))
			//{
			_chunkLocations[coords] = data;
			//}
			//else
			//{
			//	_chunkLocations.Add(coords, data);
			//}
		}

		internal AnvilRegionHeaderChunkData[] GetAllChunkData()
		{
			return _chunkLocations.Values.ToArray();
		}
	}

	class AnvilRegionHeaderChunkData
	{
		public ChunkCoordinates ChunkCoordinates { get; }

		public int LocationOffset { get; set; }

		public int SectorCount { get; set; }

		public bool ChunkExists => LocationOffset != 0 && SectorCount != 0;

		public int UpdatedTimestamp { get; set; }

		public AnvilRegionHeaderChunkData(ChunkCoordinates coords, int locationOffset, int sectorCount, int updatedTimestamp)
		{
			ChunkCoordinates = coords;
			LocationOffset = locationOffset;
			SectorCount = sectorCount;
			UpdatedTimestamp = updatedTimestamp;
		}
	}
}
