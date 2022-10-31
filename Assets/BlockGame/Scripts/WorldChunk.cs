using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;


public class WorldChunk : IComparable
{

    public int index1;
    public int index2;
    int worldChunkSize;
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
    public int size = 0;
    public static Vector2Int pos;

    public WorldChunk(int size, int wcs, int index1, int index2)
    {
        this.size = wcs;
        this.worldChunkSize = size;
        this.index1 = index1;
        this.index2 = index2;
        terrains = new TerrainChunk[size, size];
    }

    public static bool ReverseSort;

    public int CompareTo(object obj)
    {
        WorldChunk other = (WorldChunk)obj;
        if (!ReverseSort)
        {
            return (new Vector2(other.index1 * worldChunkSize * size, other.index2 * worldChunkSize * size) - pos).magnitude.CompareTo((new Vector2(index1 * worldChunkSize * size, index2 * worldChunkSize * size) - pos).magnitude);
        }
        else
        {
            int m1 = Mathf.Abs(index1 * worldChunkSize * size - pos.x);
            int m2 = Mathf.Abs(index2 * worldChunkSize * size - pos.y);
            int m3 = Mathf.Abs(other.index1 * worldChunkSize * size - pos.x);
            int m4 = Mathf.Abs(other.index2 * worldChunkSize * size - pos.y);
            return (m1 > m2 ? m1 : m2).CompareTo(m3 > m4 ? m3 : m4);
        }
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
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                terrains[i, ii].Load(index1 * worldChunkSize + i, index2 * worldChunkSize + ii);
            }
        }
    }

    public void AddTerrain(TerrainChunk terrain, int i, int ii)
    {
        terrains[i,ii] = terrain;
        terrain.Load(index1 * worldChunkSize + i, index2 * worldChunkSize + ii);
        terrain.worldChunk = this;
    }
    public bool StructuresFinished;
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
                        chunk.heightsLoaded -= chunk.heightsLoaded <= 0 ? 0 : 1;
                    }
                    if (areChunksLoaded)
                    {
                        chunk.chunksLoaded -= chunk.chunksLoaded <= 0 ? 0 : 1;
                    }
                    if (areStructuresLoaded)
                    {
                        if (!(i == 1 && ii == 1) && chunk.areStructuresLoaded)
                        {
                            chunk.structuresLoaded -= chunk.structuresLoaded <= 0 ? 0 : 1;
                            for (int iii = 0; iii < 3; iii++)
                            {
                                for (int iiii = 0; iiii < 3; iiii++)
                                {
                                    WorldChunk wc = chunk.chunks[iii * 3 + iiii];
                                    wc.structuresLoaded -= !((wc.index1 == chunk.index1 - 1 + iii && wc.index2 == chunk.index2 - 1 + iiii)) ? 0 : wc.structuresLoaded <= 0 ? 0 : 1;
                                }
                            }
                            chunk.areStructuresLoaded = false;
                            if (StructuresFinished)
                            {
                                World.world.ReloadStructures(chunk);
                            }
                        }else
                        {

                            chunk.structuresLoaded -= chunk.structuresLoaded <= 0 ? 0 : 1;
                        }
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
        graphicsLoaded = false;
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                terrains[i, ii].Unload();
            }
        }
        StructuresFinished = true;
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
    public bool loading;

    public void AddHeights()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                if (chunks[i * 3 + ii].index1 == index1 - 1 + i && chunks[i * 3 + ii].index2 == index2 - 1 + ii)
                {
                    chunks[i * 3 + ii].heightsLoaded++;
                }
            }
        }
    }

    public void LoadHeights()
    {
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
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
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                terrains[i, ii].LoadChunks();
            }
        }
        areChunksLoaded = true;
        done = true;
    }
    public void LoadVoxelChunks()
    {
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
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

    public void LoadStructures()
    {
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                terrains[i, ii].LoadStructures();
            }
        }
        areStructuresLoaded = true;
        done = true;
        loading = false;
    }

    public static Vector2Int[] loadOrder;
    public static Vector2Int[] loadOrderReverse;
    public bool graphicsLoaded = false;

    public void LoadGraphics()
    {
        for (int i = 0; i < loadOrderReverse.Length; i++)
        {
            Vector2Int v = loadOrderReverse[i];
            terrains[v.x, v.y].SortVoxelChunks();
            terrains[v.x, v.y].LoadGraphicsDown();
        }
        /*for (int i = 0; i < loadOrder.Length; i++)
        {
            Vector2Int v = loadOrder[i];
            terrains[v.x, v.y].LoadGraphicsUp();
        }*/
        for (int i = 0; i < meshData.Count; i++)
        {
            meshData[i].Normalize();
        }
        graphicsLoaded = true;
        done = true;
    }
    // cool physics

    public void SimulatePlayer(Player p)
    {
        Vector3 pos = p.transform.position;
        int i = Mathf.FloorToInt(pos.x) + p.wp.posIndex.x+worldChunkSize*size/2-index1* worldChunkSize * size;
        int ii = Mathf.FloorToInt(pos.z) + p.wp.posIndex.z + worldChunkSize * size / 2 - index2 * worldChunkSize * size;
        WorldChunk terrain = null;
        bool should = true;
        while (should)
        {
            int ier = 1;
            int iier = 1;
            should = false;
            if (i < 0)
            {
                should = true;
                i += worldChunkSize * size;
                ier--;
            }
            if (i >= worldChunkSize * size)
            {
                should = true;
                i -= worldChunkSize * size;
                ier++;
            }
            if (ii < 0)
            {
                should = true;
                ii += worldChunkSize * size;
                iier--;
            }
            if (ii >= worldChunkSize * size)
            {
                should = true;
                ii -= worldChunkSize * size;
                iier++;
            }
            terrain = terrain == null ? chunks[ier * 3 + iier] : terrain.chunks[ier * 3 + iier];
        }
        p.chunk = terrain;
        if (!terrain.graphicsLoaded)
        {
            return;
        }
        int oer = Mathf.FloorToInt((float)i / size);
        int ooer = Mathf.FloorToInt((float)ii / size);
        int iii = Mathf.FloorToInt(pos.y) + p.wp.posIndex.y - 1;
        TerrainChunk tc = terrain.terrains[oer, ooer];
        if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i-oer*size,ii-ooer*size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1)
        {
            p.OnGround = true;
            pos += new Vector3(0, (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f)), 0);
            p.velocity.y = 0;
        }
        if (VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size, iii+2)] == 1 && pos.y + p.wp.posIndex.y+0.75f > iii+2)
        {
            p.OnGround = true;
            pos += new Vector3(0, (iii +2 - (pos.y + p.wp.posIndex.y +0.75f)), 0);
            p.velocity.y = 0;
        }

        float characterWidth = 0.25f;
        float  f = terrain.index1 * worldChunkSize * size + i + 1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii + 1)] == 1 | (pos.y+p.wp.posIndex.y+0.75f > iii+2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii + 2)] == 1) && f < 0)
        {
            if (Vector3.Dot(new Vector3(-1, 0, 0), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.x += f;
        }else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f < -0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii + 2)] == 1) && f > 0)
        {
            if (Vector3.Dot(new Vector3(1, 0, 0), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.x += f;
        }else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f > 0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index2 * worldChunkSize * size + ii + 1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii + 2)] == 1) && f < 0)
        {
            if (Vector3.Dot(new Vector3(0, 0, -1), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.z += f;
        }else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f < -0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii + 2)] == 1) && f > 0)
        {
            if (Vector3.Dot(new Vector3(0, 0, 1), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.z += f;
        }else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f > 0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        float ff = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii + 2)] == 1) && (f > 0 && ff > 0))
        {
            if (Mathf.Abs(f) > Mathf.Abs(ff))
            {
                pos.z += ff;
            }
            else
            {
                pos.x += f;
            }
        }
        else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f > 0.001f && ff > 0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i+1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size+1, ii - ooer * size - 1, iii + 2)] == 1) && (f < 0 && ff > 0))
        {
            if (Mathf.Abs(f) > Mathf.Abs(ff))
            {
                pos.z += ff;
            }
            else
            {
                pos.x += f;
            }
        }
        else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f < -0.001f && ff > 0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i + 1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii+1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii + 2)] == 1) && (f < 0 && ff < 0))
        {
            if (Mathf.Abs(f) > Mathf.Abs(ff))
            {
                pos.z += ff;
            }
            else
            {
                pos.x += f;
            }
        }
        else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f < -0.001f && ff < -0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii + 1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii)] == 1) | VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii + 2)] == 1) && (f > 0 && ff < 0))
        {
            if (Mathf.Abs(f) > Mathf.Abs(ff))
            {
                pos.z += ff;
            }
            else
            {
                pos.x += f;
            }
        }
        else if (p.velocity.y <= 0 && VoxelChunk.PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f > 0.001f && ff < -0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        p.transform.position = pos;

    }


}
