using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using System.Threading;


public class WorldChunk
{

    public int index1;
    public int index2;
    int size;
    TerrainChunk[,] terrains;
    public int indexOffset = 0;
    int heightsLoaded = 0;
    int chunksLoaded = 0;
    int structuresLoaded = 0;
    bool areHeightsLoaded = false;
    bool areChunksLoaded = false;
    bool areStructuresLoaded = false;
    public bool unloading;
    public WorldChunk[] chunks = new WorldChunk[3 * 3];

    public List<Chunk> graphics = new List<Chunk>();
    public List<MeshData> meshData = new List<MeshData>();
    public int meshesLoaded = 0;

    public WorldChunk(int size, int index1, int index2)
    {
        this.size = size;
        this.index1 = index1;
        this.index2 = index2;
        terrains = new TerrainChunk[size, size];
    }

    public void Load(int index1, int index2)
    {
        this.index1 = index1;
        this.index2 = index2;
        heightsLoaded = 0;
        chunksLoaded = 0;
        structuresLoaded = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                WorldChunk chunk = chunks[i * 3 + ii];
                if (chunk.index1 == index1 - 1 + i && chunk.index2 == index2 - 1 + ii)
                {
                    if (chunks[i * 3 + ii].AreHeightsLoaded())
                    {
                        heightsLoaded++;
                    }
                    if (chunks[i * 3 + ii].AreChunksLoaded())
                    {
                        chunksLoaded++;
                    }
                    if (chunks[i * 3 + ii].AreStructuresLoaded())
                    {
                        structuresLoaded++;
                    }
                }
            }
        }
    }

    public void AddTerrain(TerrainChunk terrain, int i, int ii)
    {
        terrains[i,ii] = terrain;
        terrain.worldChunk = this;
    }

    public void Unload()
    {
        unloading = true;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                WorldChunk chunk = chunks[i * 3 + ii];
                if (chunk.index1 == index1-1+i && chunk.index2 == index2 - 1 + ii)
                {
                    if (areHeightsLoaded)
                    {
                        chunk.heightsLoaded--;
                    }
                    if (areChunksLoaded)
                    {
                        chunk.chunksLoaded--;
                    }
                    if (areStructuresLoaded)
                    {
                        chunk.structuresLoaded--;
                    }
                }
            }
        }
        heightsLoaded = 0;
        chunksLoaded = 0;
        structuresLoaded = 0;
        areHeightsLoaded = false;
        areChunksLoaded = false;
        areStructuresLoaded = false;
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].Unload();
            }
        }
        meshesLoaded = 0;
    }
    


    public int HeightsLoaded()
    {
        return heightsLoaded;
    }
    public int ChunksLoaded()
    {
        return chunksLoaded;
    }
    public int StructuresLoaded()
    {
        return structuresLoaded;
    }

    public bool AreHeightsLoaded()
    {
        return areHeightsLoaded;
    }
    public bool AreChunksLoaded()
    {
        return areChunksLoaded;
    }
    public bool AreStructuresLoaded()
    {
        return areStructuresLoaded;
    }

    public Thread thread;
    public bool done = false;

    public void AddHeights()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                if (chunks[i*3+ii].index1 == index1 - 1 + i && chunks[i * 3 + ii].index2 == index2 - 1 + ii)
                {
                    chunks[i*3+ii].heightsLoaded++;
                }
            }
        }
    }

    public void Load()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].Load(index1 * size + i, index2 * size + ii);
            }
        }
    }

    public void LoadHeights()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].LoadHeights();
            }
        }
        areHeightsLoaded = true;
        done = true;
    }

    public void AddChunks()
    {
        for (int i = 0; i < 9; i++)
        {
            chunks[i].chunksLoaded++;
        }
    }

    public void LoadChunks()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].LoadChunks();
            }
        }
        areChunksLoaded = true;
        done = true;
    }
    public void LoadVoxelChunks()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].LoadVoxelChunks();
            }
        }
    }

    public void AddStructures()
    {
        for (int i = 0; i < 9; i++)
        {
            chunks[i].structuresLoaded++;
        }
    }
    public bool AreStructuresLoading()
    {
        for (int i = 0; i < 9; i++)
        {
            if (chunks[i].loading)
            {
                return true;
            }
        }
        return false;
    }

    public bool loading;
    public void LoadStructures()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                terrains[i, ii].LoadStructures();
            }
        }
        areStructuresLoaded = true;
        done = true;
    }

    public static Vector2Int[] loadOrder;
    public static Vector2Int[] loadOrderReverse;
    public void LoadGraphics()
    {
        for (int i = 0; i < loadOrder.Length; i++)
        {
            Vector2Int v = loadOrder[i];
            terrains[v.x,v.y].LoadGraphicsDown();
        }
        for (int i = 0; i < loadOrderReverse.Length; i++)
        {
            Vector2Int v = loadOrderReverse[i];
            terrains[v.x, v.y].LoadGraphicsUp();
        }
        for (int i = 0; i < meshData.Count; i++)
        {
            meshData[i].Normalize();
        }
        done = true;
    }



}
