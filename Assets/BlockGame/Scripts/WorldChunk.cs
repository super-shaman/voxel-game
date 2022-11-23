using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;

public class WorldChunk : IComparable
{

    public int index1;
    public int index2;
    int worldChunkSize;
    public int size = 0;
    TerrainChunk[,] terrains;
    int structuresLoaded = 0;
    int graphicsLoaded = 0;
    bool areHeightsLoaded = false;
    public bool areStructuresLoaded = false;
    public bool areGraphicsLoaded = false;
    public bool unloading;
    public bool compressed = false;
    public bool StructuresFinished;
    public bool structuresReloading = false;
    public bool NeedsToLoad = false;
    public bool saved = false;
    public bool loadedFromDisk = false;

    public WorldChunk[] chunks = new WorldChunk[3 * 3];

    public List<Chunk> graphics = new List<Chunk>();
    public List<MeshData> meshData = new List<MeshData>();

    public ChunkBatch batch = null;
    public static Vector2Int pos;
    public byte lodLevel = 255;
    public int VertexCount;
    public int IndexCount;

    public bool batched()
    {
        return batch != null;
    }

    public void Batch(ChunkBatch batch)
    {
        this.batch = batch;
    }

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
        structuresLoaded = 0;
        lodLevel = 255;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                WorldChunk chunk = chunks[i * 3 + ii];
                if (chunk.index1 == index1 - 1 + i && chunk.index2 == index2 - 1 + ii)
                {
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

    public void Unload()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                WorldChunk chunk = chunks[i * 3 + ii];
                if (chunk.index1 == index1-1+i && chunk.index2 == index2 - 1 + ii)
                {
                    for (int iii = 0; iii < 3; iii++)
                    {
                        for (int iiii = 0; iiii < 3; iiii++)
                        {
                            WorldChunk wc = chunk.chunks[iii * 3 + iiii];
                            if (wc.index1 == chunk.index1 - 1 + iii && wc.index2 == chunk.index2 - 1 + iiii)
                            {
                                if (wc.compressed && !wc.unloading)
                                {
                                    wc.LoadFromDisk();
                                    wc.Decompress();
                                }
                            }
                        }
                    }
                    if (areGraphicsLoaded)
                    {
                        chunk.graphicsLoaded -= chunk.graphicsLoaded <= 0 ? 0 : 1;
                    }
                    if (areStructuresLoaded && chunk.areStructuresLoaded)
                    {
                        for (int iii = 0; iii < 3; iii++)
                        {
                            for (int iiii = 0; iiii < 3; iiii++)
                            {
                                WorldChunk wc = chunk.chunks[iii * 3 + iiii];
                                if (wc.index1 == chunk.index1 - 1 + iii && wc.index2 == chunk.index2 - 1 + iiii)
                                {
                                    wc.structuresLoaded -= wc.structuresLoaded <= 0 ? 0 : 1;
                                }
                            }
                        }
                        chunk.areStructuresLoaded = false;
                        if (chunk.StructuresFinished && !chunk.unloading)
                        {
                            World.world.ReloadStructures(chunk);
                        }
                    }
                }
            }
        }
        saved = false;
        structuresLoaded = 0;
        graphicsLoaded = 0;
        VertexCount = 0;
        IndexCount = 0;
        areHeightsLoaded = false;
        areStructuresLoaded = false;
        areGraphicsLoaded = false;
        compressed = false;
        NeedsToLoad = false;
        structuresReloading = false;
        loadedFromDisk = false;
        batch = null;
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                terrains[i, ii].Unload();
            }
        }
        StructuresFinished = false;
    }

    public bool CanLoad()
    {
        for (int i = 0; i < 9; i++)
        {
            if (chunks[i].compressed)
            {
                return false;
            }
        }
        return true;
    }
    public bool allLoadedFromDisk()
    {
        for (int i = 0; i < 9; i++)
        {
            if (!chunks[i].loadedFromDisk)
            {
                return false;
            }
        }
        return true;
    }
    public bool MakeLoadable()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                WorldChunk wc = chunks[i * 3 + ii];
                if ((wc.index1 == index1 - 1 + i && wc.index2 == index2 - 1 + ii) && wc.CanLoadHeights() && wc.StructuresLoaded() == 9)
                {
                    wc.NeedsToLoad = true;
                    if (wc.compressed)
                    {
                        wc.LoadFromDisk();
                        wc.Decompress();
                    }
                }
            }
        }
        return true;
    }
    public bool Loading()
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

    public bool AreAllStructuresLoaded()
    {
        for (int i = 0; i < 9; i++)
        {
            if (chunks[i].structuresLoaded != 9)
            {
                return false;
            }
        }
        return true;
    }

    public bool AreHeightsLoaded()
    {
        return areHeightsLoaded;
    }
    public bool AreStructuresLoaded()
    {
        return areStructuresLoaded;
    }
    
    public int StructuresLoaded()
    {
        return structuresLoaded;
    }
    public int GraphicsLoaded()
    {
        return graphicsLoaded;
    }

    public void FinishChunk()
    {
        areStructuresLoaded = false;
        areHeightsLoaded = true;

    }

    public Thread thread;
    public bool done = false;
    public bool loading;
    
    public void AddStructures()
    {
        for (int i = 0; i < 9; i++)
        {
            chunks[i].structuresLoaded++;
        }
    }
    public void AddGraphics()
    {
        for (int i = 0; i < 9; i++)
        {
            chunks[i].graphicsLoaded++;
        }
    }

    public bool CanLoadHeights()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {

                if (!(chunks[i * 3 + ii].index1 == index1 - 1 + i && chunks[i * 3 + ii].index2 == index2 - 1 + ii))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool CanLoadStructures()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                if (!chunks[i*3+ii].areHeightsLoaded)
                {
                    return false;
                }
            }
        }
        return true;
    }
    public bool Saved()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {

                if (!chunks[i * 3 + ii].saved)
                {
                    return false;
                }
            }
        }
        return true;
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



    static byte[] compressor = new byte[258];
    static int compressorCount = 0;

    public bool CompressChunk(VoxelChunk vc)
    {
        byte prev = 0;
        byte a = 0;
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                for (int iii = 0; iii < size; iii++)
                {
                    byte t = vc.GetTypeFast(i, ii, iii);
                    if (a == 0)
                    {
                        prev = t;
                        a++;
                    }
                    else if (prev == t)
                    {
                        a++;
                    }
                    else
                    {
                        compressor[compressorCount] = prev;
                        compressor[compressorCount+1] = a;
                        compressorCount += 2;
                        prev = t;
                        a = 1;
                    }
                    if (a == 255)
                    {
                        compressor[compressorCount] = prev;
                        compressor[compressorCount + 1] = a;
                        compressorCount += 2;
                        prev = t;
                        a = 0;
                    }
                    if (compressorCount >= 256)
                    {
                        compressorCount = 0;
                        return false;
                    }
                }
            }
        }
        if (a > 0)
        {
            compressor[compressorCount] = prev;
            compressor[compressorCount + 1] = a;
            compressorCount += 2;
        }
        return true;
    }
    public void DecompressChunk(VoxelChunk vc)
    {
        int aa = 0;
        for (int i = 0; i < compressorCount; i += 2)
        {
            int t = compressor[i];
            int a = compressor[i + 1];
            for (int ii = 0; ii < a; ii++)
            {
                vc.SetTypeFast(aa, t);
                aa++;
            }
        }
    }
    
    static byte[] byteArray = new byte[8 * 8*8 * 4];
    static int[] IntBuffer = new int[1];

    public void SaveToDisk()
    {
        if (!File.Exists("worldSave/data" + index1 + "_" + index2 + ".bin"))
        {
            using (FileStream fs = new FileStream("worldSave/data" + index1 + "_" + index2 + ".bin", FileMode.Create, FileAccess.Write))
            {
                while (!fs.CanWrite)
                {
                    Thread.Sleep(1);
                }
                for (int i = 0; i < worldChunkSize; i++)
                {
                    for (int ii = 0; ii < worldChunkSize; ii++)
                    {
                        TerrainChunk tc = terrains[i, ii];
                        Buffer.BlockCopy(tc.heights, 0, byteArray, 0, 8 * 8 * 4);
                        fs.Write(byteArray, 0, 8 * 8 * 4);
                        IntBuffer[0] = tc.loadedChunks.Count;
                        Buffer.BlockCopy(IntBuffer, 0, byteArray, 0, 4);
                        fs.Write(byteArray, 0, 4);

                        for (int iii = 0; iii < tc.loadedChunks.Count; iii++)
                        {
                            VoxelChunk vc = tc.loadedChunks[iii];
                            if (CompressChunk(vc))
                            {
                                fs.WriteByte(1);
                                IntBuffer[0] = compressorCount;
                                Buffer.BlockCopy(IntBuffer, 0, byteArray, 0, 4);
                                fs.Write(byteArray, 0, 4);
                                IntBuffer[0] = vc.index3;
                                Buffer.BlockCopy(IntBuffer, 0, byteArray, 0, 4);
                                fs.Write(byteArray, 0, 4);
                                fs.Write(compressor, 0, compressorCount);
                                compressorCount = 0;
                            }
                            else
                            {

                                fs.WriteByte(0);
                                IntBuffer[0] = vc.index3;
                                Buffer.BlockCopy(IntBuffer, 0, byteArray, 0, 4);
                                fs.Write(byteArray, 0, 4);
                                fs.Write(vc.types, 0, vc.types.Length);
                            }
                        }
                        tc.Unload();
                    }
                }
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
        }else
        {
            for (int i = 0; i < worldChunkSize; i++)
            {
                for (int ii = 0; ii < worldChunkSize; ii++)
                {
                    TerrainChunk tc = terrains[i, ii];
                    tc.Unload();
                }
            }
        }
        compressed = true;
    }

    public bool LoadFromDisk()
    {
        if (File.Exists("worldSave/data" + index1 + "_" + index2 + ".bin"))
        {
            using (FileStream fs = new FileStream("worldSave/data" + index1 + "_" + index2 + ".bin", FileMode.Open, FileAccess.Read))
            {
                while (!fs.CanRead)
                {
                    Thread.Sleep(1);
                }
                for (int i = 0; i < worldChunkSize; i++)
                {
                    for (int ii = 0; ii < worldChunkSize; ii++)
                    {
                        TerrainChunk tc = terrains[i, ii];
                        fs.Read(byteArray, 0, 8 * 8 * 4);
                        Buffer.BlockCopy(byteArray, 0, tc.heights, 0, 8 * 8 * 4);
                        IntBuffer[0] = tc.loadedChunks.Count;
                        fs.Read(byteArray, 0, 4);
                        Buffer.BlockCopy(byteArray, 0, IntBuffer, 0, 4);
                        int c = IntBuffer[0];
                        for (int iii = 0; iii < c; iii++)
                        {
                            int t = fs.ReadByte();
                            VoxelChunk vc = World.world.GetVoxelChunk();
                            int len = 8 * 8 * 8;
                            if (t == 1)
                            {
                                fs.Read(byteArray, 0, 4);
                                Buffer.BlockCopy(byteArray, 0, IntBuffer, 0, 4);
                                len = IntBuffer[0];
                                fs.Read(byteArray, 0, 4);
                                Buffer.BlockCopy(byteArray, 0, IntBuffer, 0, 4);
                                int h = IntBuffer[0];
                                fs.Read(compressor, 0, len);
                                compressorCount = len;
                                DecompressChunk(vc);
                                compressorCount = 0;
                                tc.LoadChunk(vc, h);
                            }
                            else
                            {
                                fs.Read(byteArray, 0, 4);
                                Buffer.BlockCopy(byteArray, 0, IntBuffer, 0, 4);
                                int h = IntBuffer[0];
                                fs.Read(vc.types, 0, len);
                                tc.LoadChunk(vc, h);
                            }
                        }
                    }
                }
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }
            saved = true;
            compressed = false;
            loadedFromDisk = true;
            return true;
        }else
        {
            saved = true;
            compressed = false;
            return false;
        }
    }

    public void Compress()
    {

        compressed = true;
        /*for (int i = 0; i < loadOrderReverse.Length; i++)
        {
            Vector2Int v = loadOrderReverse[i];
            TerrainChunk tc = terrains[v.x, v.y];
            for (int ii = 0; ii < tc.loadedChunks.Count; ii++)
            {
                VoxelChunk vc = tc.loadedChunks[ii];
                if (CompressChunk(vc))
                {
                    RLEVoxelChunk rlevc = World.world.GetRLEVoxelChunk();
                    rlevc.Load(vc.index1, vc.index2, vc.index3, tc, compressor.ToArray(), vc);
                    compressor.Clear();
                    tc.loadedChunks[ii] = rlevc;
                    tc.setVoxelChunk(rlevc);
                    vc.Unload();
                    World.world.UnloadVoxelChunk(vc);
                }
            }
           
        }*/
    }

    public void Decompress()
    {

        /*for (int i = 0; i < loadOrderReverse.Length; i++)
        {
            Vector2Int v = loadOrderReverse[i];
            TerrainChunk tc = terrains[v.x, v.y];
            for (int ii = 0; ii < tc.loadedChunks.Count; ii++)
            {
                VoxelChunk vc = tc.loadedChunks[ii];
                if (vc.getChunkType() == 1)
                {
                    RLEVoxelChunk rlevc = (RLEVoxelChunk)vc;
                    VoxelChunk vvc = World.world.GetVoxelChunk();
                    vvc.Load(rlevc.index1, rlevc.index2, rlevc.index3, tc);
                    rlevc.Decompress(vvc);
                    tc.loadedChunks[ii] = vvc;
                    tc.setVoxelChunk(vvc);
                    vc.Unload();
                    World.world.UnloadVoxelChunk(vc);
                }
            }

        }*/
        compressed = false;
    }


    public void LoadGraphics()
    {
        for (int i = worldChunkSize - 1; i >= 0; i--)
        {
            for (int ii = worldChunkSize - 1; ii >= 0; ii--)
            {
                TerrainChunk t = terrains[i, ii];
                t.SortVoxelChunks();
                t.LoadGraphicsClose();
            }
        }
        for (int i = 0; i < meshData.Count; i++)
        {
            meshData[i].Normalize();
        }
        lodLevel = 0;
        areGraphicsLoaded = true;
        done = true;
        loading = false;
        NeedsToLoad = false;
    }
    public void LoadGraphicsNoGrass()
    {
        for (int i = worldChunkSize - 1; i >= 0; i--)
        {
            for (int ii = worldChunkSize - 1; ii >= 0; ii--)
            {
                TerrainChunk t = terrains[i, ii];
                t.SortVoxelChunks();
                t.LoadGraphicsDownSuperLowQ(2);
            }
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
        lodLevel = 1;
        areGraphicsLoaded = true;
        done = true;
        loading = false;
        NeedsToLoad = false;
    }

    public void LoadGraphicsSuperLowQ()
    {
        for (int i = worldChunkSize - 1; i >= 0; i--)
        {
            for (int ii = worldChunkSize - 1; ii >= 0; ii--)
            {
                TerrainChunk t = terrains[i, ii];
                t.SortVoxelChunks();
                t.LoadGraphicsDownSuperLowQ(4);
            }
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
        lodLevel = 2;
        areGraphicsLoaded = true;
        done = true;
        loading = false;
        NeedsToLoad = false;
    }

    public void LoadGraphicsSuperSuperLowQ()
    {
        for (int i = worldChunkSize - 1; i >= 0; i--)
        {
            for (int ii = worldChunkSize - 1; ii >= 0; ii--)
            {
                TerrainChunk t = terrains[i, ii];
                t.SortVoxelChunks();
                t.LoadGraphicsDownSuperLowQ(8);
            }
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
        lodLevel = 3;
        areGraphicsLoaded = true;
        done = true;
        loading = false;
        NeedsToLoad = false;
    }

    // cool physics

    private static byte[] PhysicsBlock =
    {
        0,
        1,
        1,
        1,
        1,
        0,
        0
    };
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
        if (!terrain.areGraphicsLoaded | !terrain.CanLoad())
        {
            return;
        }
        int oer = Mathf.FloorToInt((float)i / size);
        int ooer = Mathf.FloorToInt((float)ii / size);
        int iii = Mathf.FloorToInt(pos.y) + p.wp.posIndex.y - 1;
        TerrainChunk tc = terrain.terrains[oer, ooer];
        if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i-oer*size,ii-ooer*size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1)
        {
            p.OnGround = true;
            pos += new Vector3(0, (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f)), 0);
            p.velocity.y = 0;
        }
        if (PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size, iii+2)] == 1 && pos.y + p.wp.posIndex.y+0.75f > iii+2)
        {
            p.OnGround = true;
            pos += new Vector3(0, (iii +2 - (pos.y + p.wp.posIndex.y +0.75f)), 0);
            p.velocity.y = 0;
        }

        float characterWidth = 0.25f;
        float  f = terrain.index1 * worldChunkSize * size + i + 1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii + 1)] == 1 | (pos.y+p.wp.posIndex.y+0.75f > iii+2 && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii + 2)] == 1) && f < 0)
        {
            if (Vector3.Dot(new Vector3(-1, 0, 0), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.x += f;
        }else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f < -0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii + 2)] == 1) && f > 0)
        {
            if (Vector3.Dot(new Vector3(1, 0, 0), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.x += f;
        }else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f > 0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index2 * worldChunkSize * size + ii + 1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii + 2)] == 1) && f < 0)
        {
            if (Vector3.Dot(new Vector3(0, 0, -1), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.z += f;
        }else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f < -0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii + 2)] == 1) && f > 0)
        {
            if (Vector3.Dot(new Vector3(0, 0, 1), p.velocity.normalized) < -0.75f)
            {
                p.run = false;
            }
            pos.z += f;
        }else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && f > 0.001f)
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        float ff = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii + 2)] == 1) && (f > 0 && ff > 0))
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
        else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size-1, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f > 0.001f && ff > 0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i+1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z - characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size+1, ii - ooer * size - 1, iii + 2)] == 1) && (f < 0 && ff > 0))
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
        else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size - 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f < -0.001f && ff > 0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i + 1 - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x + characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii+1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii + 2)] == 1) && (f < 0 && ff < 0))
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
        else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size + 1, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f < -0.001f && ff < -0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        f = terrain.index1 * worldChunkSize * size + i - worldChunkSize * size / 2 - p.wp.posIndex.x - (pos.x - characterWidth);
        ff = terrain.index2 * worldChunkSize * size + ii + 1 - worldChunkSize * size / 2 - p.wp.posIndex.z - (pos.z + characterWidth);
        if ((pos.y + p.wp.posIndex.y - 1 <= iii + 0.99f && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii)] == 1) | PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii + 1)] == 1 | (pos.y + p.wp.posIndex.y + 0.75f > iii + 2 && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii + 2)] == 1) && (f > 0 && ff < 0))
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
        else if (p.velocity.y <= 0 && PhysicsBlock[tc.GetBlock(i - oer * size - 1, ii - ooer * size + 1, iii)] == 1 && pos.y + p.wp.posIndex.y - 1 <= iii + 1 && (f > 0.001f && ff < -0.001f))
        {
            pos.y += (iii + 1 - (pos.y + p.wp.posIndex.y - 0.999f));
            p.velocity.y = 0;
            p.OnGround = true;
        }

        p.transform.position = pos;

    }


}
