using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class TerrainChunk
{

    public int index1;
    public int index2;
    int size;
    public float[] heights;
    int chunkHeight;
    int chunkDepth;
    public TerrainChunk[] chunks = new TerrainChunk[3 * 3];
    VoxelChunk[] voxelChunks;
    public List<VoxelChunk> loadedChunks = new List<VoxelChunk>();
    public WorldChunk worldChunk;

    public TerrainChunk(int size, int chunkHeight, int chunkDepth)
    {
        this.size = size;
        heights = new float[size * size];
        this.chunkHeight = chunkHeight;
        this.chunkDepth = chunkDepth;
        voxelChunks = new VoxelChunk[this.chunkHeight+ chunkDepth];
    }

    public void Load(int index1, int index2)
    {
        this.index1 = index1;
        this.index2 = index2;
    }

    public void Unload()
    {
        for (int i = 0; i < loadedChunks.Count; i++)
        {
            VoxelChunk chunk = loadedChunks[i];
            chunk.Unload();
            World.world.UnloadVoxelChunk(chunk);
            voxelChunks[chunkDepth + chunk.index3] = null;
        }
        loadedChunks.Clear();

    }

    float minH;
    float maxH;

    public void LoadHeights()
    {
        minH = float.MaxValue;
        maxH = float.MinValue;
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                float height = (float)WorldNoise.GetHeight(index1 * size + i, index2 * size + ii,0)*512;
                minH = height < minH ? height : minH;
                maxH = height > maxH ? height : maxH;
                heights[i * size + ii] = height;
            }
        }
        maxH = maxH < 0 ? 0 : maxH;
    }
    
    public void LoadVoxelChunks()
    {
        float minH = this.minH;
        float maxH = this.maxH;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                TerrainChunk chunk = chunks[i * 3 + ii];
                minH = chunk.minH < minH ? chunk.minH : minH;
                maxH = chunk.maxH > maxH ? chunk.maxH : maxH;
            }
        }
        int minIndex = Mathf.FloorToInt(minH / size);
        int maxIndex = Mathf.CeilToInt(maxH / size);
        minIndex = minIndex < -chunkDepth ? -chunkDepth : minIndex;
        maxIndex = maxIndex > chunkHeight ? chunkHeight : maxIndex;
        for (int i = minIndex; i <= maxIndex; i++)
        {
            VoxelChunk chunk = World.world.GetVoxelChunk();
            chunk.Load(index1, index2, i, this);
            voxelChunks[chunkDepth + i] = chunk;
            loadedChunks.Add(chunk);
            for (int o = 0; o < 3; o++)
            {
                for (int oo = 0; oo < 3; oo++)
                {
                    for (int ooo = 0; ooo < 3; ooo++)
                    {
                        VoxelChunk c = chunks[o * 3 + oo].voxelChunks[chunkDepth + chunk.index3 - 1 + ooo];
                        if (c != null)
                        {
                            chunk.chunks[o * 3 * 3 + oo * 3 + ooo] = c;
                            c.chunks[(2 - o) * 3 * 3 + (2 - oo) * 3 + 2 - ooo] = chunk;
                        }
                    }
                }
            }
        }
    }

    public void LoadChunks()
    {
        for (int i = 0; i < loadedChunks.Count; i++)
        {
            loadedChunks[i].LoadVoxels();
        }
    }


    public void LoadGraphics()
    {
        for (int i = loadedChunks.Count - 1; i >= 0; i--)
        {
            VoxelChunk chunk = loadedChunks[i];
            chunk.LoadGraphicsDownFast();
        }
        for (int i = 0; i < loadedChunks.Count; i++)
        {
            VoxelChunk chunk = loadedChunks[i];
            chunk.LoadGraphicsUpFast();
        }
    }

    VoxelChunk LoadChunk(int iii)
    {
        if (voxelChunks[chunkDepth+iii] != null)
        {
            return null;
        }
        VoxelChunk chunk = World.world.GetVoxelChunk();
        chunk.Load(index1, index2, iii, this);
        chunk.LoadVoxels();
        voxelChunks[chunkDepth + iii] = chunk;
        loadedChunks.Add(chunk);
        for (int o = 0; o < 3; o++)
        {
            for (int oo = 0; oo < 3; oo++)
            {
                for (int ooo = 0; ooo < 3; ooo++)
                {
                    VoxelChunk c = chunks[o * 3 + oo].voxelChunks[chunkDepth + chunk.index3 - 1 + ooo];
                    if (c != null)
                    {
                        chunk.chunks[o * 3 * 3 + oo * 3 + ooo] = c;
                        c.chunks[(2 - o) * 3 * 3 + (2 - oo) * 3 + 2 - ooo] = chunk;
                    }
                }
            }
        }
        return chunk;
    }

    void SetBlock(int i, int ii, int iii, int type)
    {
        int iiier = Mathf.FloorToInt((float)iii / size);
        int ier = 1;
        int iier = 1;
        if (i < 0)
        {
            i += size;
            ier--;
        }
        if (i >= size)
        {
            i -= size;
            ier++;
        }
        if (ii < 0)
        {
            ii += size;
            iier--;
        }
        if (ii >= size)
        {
            ii -= size;
            iier++;
        }
        TerrainChunk terrain = chunks[ier * 3 + iier];
        VoxelChunk chunk = terrain.voxelChunks[chunkDepth + iiier];
        if (chunk != null)
        {
            terrain.chunks[3].LoadChunk(iiier);
            terrain.chunks[5].LoadChunk(iiier);
            terrain.chunks[7].LoadChunk(iiier);
            terrain.chunks[1].LoadChunk(iiier);
            terrain.LoadChunk(iiier + 1);
            terrain.LoadChunk(iiier - 1);
            chunk.SetType(i, ii, iii - iiier * size, type);
        }else
        {
            chunk = terrain.LoadChunk(iiier);
            terrain.chunks[3].LoadChunk(iiier);
            terrain.chunks[5].LoadChunk(iiier);
            terrain.chunks[7].LoadChunk(iiier);
            terrain.chunks[1].LoadChunk(iiier);
            terrain.LoadChunk(iiier + 1);
            terrain.LoadChunk(iiier - 1);
            chunk.SetType(i, ii, iii - iiier * size, type);
        }
    }

    void SpawnTree(int i, int ii, int h)
    {

        for (int o = -2; o < 3; o++)
        {
            for (int oo = -2; oo < 3; oo++)
            {
                int ier = i+o;
                int iier = ii + oo;
                SetBlock(ier, iier, h + 8, 4);
            }
        }
        for (int o = -2; o < 3; o++)
        {
            for (int oo = -2; oo < 3; oo++)
            {
                int ier = i + o;
                int iier = ii + oo;
                SetBlock(ier, iier, h + 9, 4);
            }
        }
        for (int o = -1; o < 2; o++)
        {
            for (int oo = -1; oo < 2; oo++)
            {
                int ier = i + o;
                int iier = ii + oo;
                SetBlock(ier, iier, h + 10, 4);
            }
        }
        for (int o = -1; o < 2; o++)
        {
            for (int oo = -1; oo < 2; oo++)
            {
                int ier = i + o;
                int iier = ii + oo;
                SetBlock(ier, iier, h + 11, 4);
            }
        }
        for (int o = 1; o < 8+3; o++)
        {
            SetBlock(i, ii, h + o, 3);
        }
    }
    void SpawnGrass(int i, int ii, int h)
    {

        SetBlock(i, ii, h + 1, 5);
    }

    public void LoadStructures()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                float height = heights[i * size + ii];
                if (height > 0 && (WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 0, 0) + 1) * 64 < 1)
                {
                    SpawnTree(i, ii, Mathf.FloorToInt(height));
                } else if (height > 0)// && (WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 1, 0) + 1) * 3 < 1)
                {
                    SpawnGrass(i,ii, Mathf.FloorToInt(height));
                }
            }
        }
    }

}