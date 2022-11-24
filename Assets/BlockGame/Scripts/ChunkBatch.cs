using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkBatch
{
    public List<WorldChunk> batchedChunks = new List<WorldChunk>();
    public Chunk chunk;
    public int vertCounter = 0;
    public ChunkBatch()
    {
    }

    public bool AddChunk(WorldChunk chunk)
    {
        if (batchedChunks.Count == 0)
        {
            if (chunk.graphics.Count == 1)
            {
                batchedChunks.Add(chunk);
                vertCounter += chunk.VertexCount;
                return true;
            }else
            {
                return false;
            }
        }else
        {
            WorldChunk origin = batchedChunks[0];
            if ((origin.index1 - chunk.index1 >= -1 && origin.index1 - chunk.index1 < 2 && origin.index2 - chunk.index2 >= -1 && origin.index2 - chunk.index2 < 2) && chunk.graphics.Count == 1 && chunk.VertexCount + vertCounter < MeshData.maxVertices)
            {
                batchedChunks.Add(chunk);
                vertCounter += chunk.VertexCount;
                return true;
            }else
            {
                return false;
            }
        }
    }

    public void Unload()
    {
        for (int i = 0; i < batchedChunks.Count; i++)
        {
            batchedChunks[i].batch = null;
        }
    }
    
}