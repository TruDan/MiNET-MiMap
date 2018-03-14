using System.Numerics;
using MiNET.Utils;
using MiNET.Worlds;

namespace MiMap.AnvilTileGenerator.Worlds
{
	public class VoidWorldProvider : IWorldProvider
	{
		public void Initialize() {}
	    public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates, bool cacheOnly = false)
	    {
	        throw new System.NotImplementedException();
	    }
        
	    public string GetName()
	    {
	        throw new System.NotImplementedException();
	    }

	    public ChunkColumn GenerateChunkColumn(ChunkCoordinates chunkCoordinates)
		{
			var chunk = new ChunkColumn();
			chunk.x = chunkCoordinates.X;
			chunk.z = chunkCoordinates.Z;
			chunk.isDirty = true;
			return chunk;
		}

		public Vector3 GetSpawnPoint()
		{
			return new Vector3(0, 0, 0);
		}

		public long GetTime()
		{
			return 0;
		}

	    public long GetDayTime()
	    {
	        throw new System.NotImplementedException();
	    }

	    public int SaveChunks()
        {
            return 0;
        }

	    public bool HaveNether()
	    {
	        throw new System.NotImplementedException();
	    }

	    public bool HaveTheEnd()
	    {
	        throw new System.NotImplementedException();
	    }

	    public bool IsCaching { get; } = false;
	}
}
