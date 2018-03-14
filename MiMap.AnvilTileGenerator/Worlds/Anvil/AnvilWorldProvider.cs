using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using fNbt;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiMap.AnvilTileGenerator.Worlds.Anvil
{
	public class AnvilWorldProvider : IWorldProvider
	{
		private static readonly Regex RegionFilenameRegex = new Regex("^r\\.(?'x'-?[0-9]*)\\.(?'z'-?[0-9]*)\\.mca$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
			
		public ChunkCoordinates[] Regions => _regionCache.Keys.ToArray();

		private ConcurrentDictionary<ChunkCoordinates, AnvilRegion> _regionCache =
			new ConcurrentDictionary<ChunkCoordinates, AnvilRegion>();

		public string BasePath { get; private set; }

		public LevelInfo LevelInfo { get; private set; }

		private Queue<Block> LightSources { get;  set; }


		public AnvilWorldProvider(string basePath)
		{
			BasePath = basePath;
		}

		private bool _isInitialized = false;
		private object _initializeSync = new object();

		public void Initialize()
		{
			if (_isInitialized)
			{
				return; // Quick exit
			}

			lock (_initializeSync)
			{
				if (_isInitialized)
				{
					return;
				}

				var file = new NbtFile();
				file.LoadFromFile(Path.Combine(BasePath, "level.dat"));
				NbtTag dataTag = file.RootTag["Data"];
				LevelInfo = new LevelInfo(dataTag);

				LoadAvailableRegions();

				_isInitialized = true;
			}
		}
		

		public string GetName()
		{
			return LevelInfo.LevelName;
		}

		private void LoadAvailableRegions()
		{
			string path = Path.Combine(BasePath, "region");
			//Debug.WriteLine($"Loading all regions from {path}");

			if (!Directory.Exists(path))
			{
				return;
			}

			string[] files = Directory.GetFiles(path);

			foreach (string regionFilePath in files)
			{
				string regionFile = regionFilePath.Replace(path + Path.DirectorySeparatorChar, "");
				try
				{
					Match match = RegionFilenameRegex.Match(regionFile);
					if (match.Success)
					{
						int x = int.Parse(match.Groups["x"].Value);
						int z = int.Parse(match.Groups["z"].Value);

						var region = new AnvilRegion(this, x, z);
						_regionCache.TryAdd(new ChunkCoordinates(x, z), region);
					}
				}
				catch (Exception ex) {}
			}
		}

		private AnvilRegion GenerateRegion(ChunkCoordinates coords)
		{
			int x = coords.X >> 5;
			int z = coords.Z >> 5;
			var regionCoords = new ChunkCoordinates(x, z);

			//Debug.WriteLine($"Generating AnvilRegion {x},{z}");

			var region = new AnvilRegion(this, x, z);
			_regionCache[regionCoords] = region;
			return region;
		}

		internal AnvilRegion GetRegion(ChunkCoordinates coords)
		{
			int x = coords.X >> 5;
			int z = coords.Z >> 5;
			var regionCoords = new ChunkCoordinates(x, z);

			AnvilRegion region;
			if (_regionCache.TryGetValue(regionCoords, out region))
			{
				if (!region.IsLoaded)
				{
					region.Load();
				}

				return region;
			}

			return GenerateRegion(coords);
		}


		public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
		{
			AnvilRegion region = GetRegion(chunkCoordinates);
			return region.GetChunk(chunkCoordinates);
		}

		private ChunkColumn GetChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			AnvilRegion region = GetRegion(chunkCoordinates);
			return region?.GetChunk(chunkCoordinates, false);
		}

		private void DeleteChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			AnvilRegion region = GetRegion(chunkCoordinates);
			region?.DeleteChunk(chunkCoordinates);
		}

		public Vector3 GetSpawnPoint()
		{
			return new Vector3(LevelInfo.SpawnX, LevelInfo.SpawnY, LevelInfo.SpawnZ);
		}

		public long GetTime()
		{
			return LevelInfo.Time;
		}

		public long GetDayTime()
		{
			throw new NotImplementedException();
		}

		public void LoadAllChunks()
		{
			foreach (AnvilRegion region in _regionCache.Values)
			{
				if (!region.IsLoaded)
				{
					region.Load();
				}
				region.LoadAllChunks();
			}
		}

		public int SaveChunks()
		{
			int count = 0;
			foreach (AnvilRegion region in _regionCache.Values)
			{
				if (region.IsLoaded)
				{
					count += region.Save();
				}
			}
			return count;
		}

		public bool HaveNether()
		{
			return false;
		}

		public bool HaveTheEnd()
		{
			return false;
		}

		public AnvilChunk[] GetAllChunks()
		{
			LoadAllChunks();
			return GetAllLoadedChunks();
		}

		public AnvilChunk[] GetAllLoadedChunks()
		{
			var chunks = new List<AnvilChunk>();

			foreach (AnvilRegion region in _regionCache.Values)
			{
				chunks.AddRange(region.GetLoadedChunks());
			}

			return chunks.ToArray();
		}

		public Queue<Block> GetLightSources()
		{
			return new Queue<Block>(LightSources);
		}

		public bool IsCaching { get; } = true;

		public int PruneAir()
		{
			var prunedChunks = 0;
			var sw = new Stopwatch();
			sw.Start();

			foreach (AnvilChunk chunkColumn in GetAllChunks())
			{
				var chunkCoordinates = new ChunkCoordinates(chunkColumn.x, chunkColumn.z);

				if (chunkColumn != null && chunkColumn.isAllAir)
				{
					var surroundingIsAir = true;

					for (int startX = chunkCoordinates.X - 1; startX <= chunkCoordinates.X + 1; startX++)
					{
						for (int startZ = chunkCoordinates.Z - 1; startZ <= chunkCoordinates.Z + 1; startZ++)
						{
							var surroundingChunkCoordinates = new ChunkCoordinates(startX, startZ);

							if (!surroundingChunkCoordinates.Equals(chunkCoordinates))
							{
								ChunkColumn surroundingChunkColumn = GetChunkColumn(surroundingChunkCoordinates);

								if (surroundingChunkColumn != null && !surroundingChunkColumn.isAllAir)
								{
									surroundingIsAir = false;
									break;
								}
							}
						}
					}

					if (surroundingIsAir)
					{
						DeleteChunkColumn(chunkCoordinates);
						prunedChunks++;
					}
				}
			}

			sw.Stop();
			Debug.WriteLine("Pruned " + prunedChunks + " in " + sw.ElapsedMilliseconds + "ms");
			return prunedChunks;
		}

		public int MakeAirChunksAroundWorldToCompensateForBadRendering()
		{
			var createdChunks = 0;
			var sw = new Stopwatch();
			sw.Start();

			foreach (AnvilChunk chunkColumn in GetAllChunks())
			{
				var chunkCoordinates = new ChunkCoordinates(chunkColumn.x, chunkColumn.z);

				if (chunkColumn != null && !chunkColumn.isAllAir)
				{
					for (int startX = chunkCoordinates.X - 1; startX <= chunkCoordinates.X + 1; startX++)
					{
						for (int startZ = chunkCoordinates.Z - 1; startZ <= chunkCoordinates.Z + 1; startZ++)
						{
							var surroundingChunkCoordinates = new ChunkCoordinates(startX, startZ);

							if (surroundingChunkCoordinates.Equals(chunkCoordinates))
							{
								continue;
							}

							ChunkColumn surroundingChunkColumn = GenerateChunkColumn(surroundingChunkCoordinates);
						}
					}
				}
			}

			sw.Stop();
			Debug.WriteLine("Created " + createdChunks + " air chunks in " + sw.ElapsedMilliseconds + "ms");
			return createdChunks;
		}
	}
}
