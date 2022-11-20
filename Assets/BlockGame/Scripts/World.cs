
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Concurrent;
using System.IO;

public class World : MonoBehaviour
{

    public Chunk ChunkGraphics;
    public Player player;
    public TextMeshProUGUI fps;
    public TextMeshProUGUI memory;
    public TextMeshProUGUI position;
    public GameObject pauseMenu;
    public float LODSize;
    public int worldChunkSize = 8;
    byte size =  8;
    int loadSize = (MainMenu.loadSize*32 + 32);
    int worldChunkLoadSize;
    int worldChunkSizer;
    int maxThreads = System.Environment.ProcessorCount/2;
    int threadOffset = 0;

    TerrainChunk[,] terrains;
    WorldChunk[,] worldChunks;
    List<WorldChunk> heightsToLoad;
    List<WorldChunk> structuresToLoad;
    List<WorldChunk> worldsToLoad;
    List<WorldChunk> loadingGraphics;
    List<WorldChunk> loadingChunks;

    public static World world;

    void Start()
    {
        if (!Directory.Exists("worldSave"))
        {
            Directory.CreateDirectory("worldSave");
        }
        Color[] colors = new Color[ChunkGraphics.mr.sharedMaterials.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Material m = ChunkGraphics.mr.sharedMaterials[i];
            if (m.mainTexture != null)
            {
                Texture2D texture = (Texture2D)m.mainTexture;
                Color color = new Color(0, 0, 0, 0);
                int aa = 0;
                for (int ii = 0; ii < texture.width; ii++)
                {
                    for (int iii = 0; iii < texture.height; iii++)
                    {
                        Color col = texture.GetPixel(ii, iii);
                        if (col.a > 0.5f)
                        {
                            color += col;
                            aa++;
                        }
                    }
                }
                colors[i] = color / aa;
                colors[i].a = 1;
            }else
            {
                colors[i] = m.color;
                colors[i].a = 1;
            }
        }
        MeshData.colors = colors;
        int[] types = new int[8 * 8*8];
        for (int i = 0; i < 8; i++)
        {
            for (int ii = 0; ii < 8; ii++)
            {
                for (int iii = 0; iii < 8; iii++)
                {
                    types[i * 8 * 8 + ii * 8 + iii] = iii >= 4 ? 0 : 1;
                }
            }
        }
        List<int> compressor = new List<int>();
        int t = types[0];
        int a = 1;
        for (int i = 1; i < types.Length; i++)
        {
            if (types[i] == t)
            {
                a++;
            }else
            {
                compressor.Add(t*512 + a);
                t = types[i];
                a = 1;
            }
        }
        compressor.Add(t * 512 + a);
        t = types[types.Length-1];
        a = 1;
        int[] decompress = new int[8 * 8*8];
        int iter = 0;
        for (int i = 0; i < compressor.Count; i++)
        {
            int c = compressor[i];
            int x = Mathf.FloorToInt((float)((double)c / 512.0));
            int y = c - x * 512;
            for (int ii = 0; ii < y; ii++)
            {
                decompress[iter] = x;
                iter++;
            }
        }
        Debug.Log(iter);
        Debug.Log(compressor.Count);
        bool same = true;
        for (int i = 0; i < decompress.Length; i++)
        {
            if (types[i] != decompress[i])
            {
                same = false;
            }
        }
        Debug.Log(same);
        Chunk.LODSize = LODSize;
        Time.timeScale = 1;
        if (!Application.isEditor)
        {
            GarbageCollector.incrementalTimeSliceNanoseconds = 0;
        }
        LoadLoadOrder();
        LoadWorld();
        for (int i = 0; i < heightsToLoad.Count; i++)
        {
            WorldChunk wc = heightsToLoad[i];
            for (int ii = i + 1; ii < heightsToLoad.Count; ii++)
            {
                WorldChunk wc2 = heightsToLoad[ii];
                if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - new Vector2()).magnitude > (new Vector2(wc2.index1 * size * worldChunkSize, wc2.index2 * size * worldChunkSize) - new Vector2()).magnitude)
                {
                    heightsToLoad[i] = wc2;
                    heightsToLoad[ii] = wc;
                    wc = wc2;
                }
            }
        }
        addGraphics = true;
        player.chunk = worldChunks[worldChunkLoadSize / 2, worldChunkLoadSize / 2];
    }

    //VoxelChunk Pooling

    List<VoxelChunk> voxelPool = new List<VoxelChunk>();
    List<int> voxelIndexPool = new List<int>();

    public void UnloadVoxelChunk(VoxelChunk chunk)
    {
        voxelIndexPool.Add(chunk.getMemIndex());
    }

    public VoxelChunk GetVoxelChunk()
    {
        if (voxelIndexPool.Count == 0)
        {
            VoxelChunk v = new VoxelChunk(size, voxelPool.Count, true);
            voxelPool.Add(v);
            return v;
        }
        else
        {
            int ind = voxelIndexPool[voxelIndexPool.Count - 1];
            voxelIndexPool.RemoveAt(voxelIndexPool.Count - 1);
            VoxelChunk chunk = voxelPool[ind];
            return chunk;
        }
    }
    
    List<WorldChunk> reloadStructures = new List<WorldChunk>();
    public void ReloadStructures(WorldChunk wc)
    {
        if (!wc.structuresReloading)
        {
            reloadStructures.Add(wc);
            wc.structuresReloading = true;
        }
    }

    //LoadOrder

    void LoadLoadOrder()
    {
        List<Vector3Int> spherePos = new List<Vector3Int>();
        for (int o = -16; o < 16 + 1; o++)
        {
            for (int oo = -16; oo < 16 + 1; oo++)
            {
                for (int oooo = -16; oooo < 16 + 1; oooo++)
                {
                    Vector3Int ver = new Vector3Int(o, oo, oooo);
                    if (ver.magnitude < 16)
                    {
                        spherePos.Add(ver);
                    }
                }
            }
        }
        TerrainChunk.sphere = spherePos.ToArray();
        System.Array.Sort(TerrainChunk.sphere, (x, y) => x.magnitude.CompareTo(y.magnitude));
    }

    //Initializing World

    public void LoadWorld()
    {
        world = this;
        worldChunkLoadSize = loadSize / worldChunkSize;
        heightsToLoad = new List<WorldChunk>(worldChunkLoadSize * worldChunkLoadSize);
        structuresToLoad = new List<WorldChunk>(worldChunkLoadSize * worldChunkLoadSize);
        worldsToLoad = new List<WorldChunk>(worldChunkLoadSize * worldChunkLoadSize);
        loadingGraphics = new List<WorldChunk>(worldChunkLoadSize * worldChunkLoadSize);
        loadingChunks = new List<WorldChunk>(worldChunkLoadSize * worldChunkLoadSize);
        worldChunkSizer = worldChunkSize * size;
        worldChunks = new WorldChunk[worldChunkLoadSize, worldChunkLoadSize];
        terrains = new TerrainChunk[loadSize, loadSize];
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk worldChunk = new WorldChunk(worldChunkSize, size, -worldChunkLoadSize / 2 + i, -worldChunkLoadSize / 2 + ii);
                worldChunks[i, ii] = worldChunk;
            }
        }
        for (int i = 0; i < loadSize; i++)
        {
            for (int ii = 0; ii < loadSize; ii++)
            {
                TerrainChunk terrain = new TerrainChunk(size, 1024 / size, 256 / size);
                terrains[i, ii] = terrain;
            }
        }
        for (int i = 0; i < loadSize; i++)
        {
            for (int ii = 0; ii < loadSize; ii++)
            {
                TerrainChunk terrain = terrains[i, ii];
                for (int o = 0; o < 3; o++)
                {
                    for (int oo = 0; oo < 3; oo++)
                    {
                        int ier = i - 1 + o;
                        int iier = ii - 1 + oo;
                        ier = ier >= loadSize ? 0 : ier < 0 ? loadSize - 1 : ier;
                        iier = iier >= loadSize ? 0 : iier < 0 ? loadSize - 1 : iier;
                        terrain.chunks[o * 3 + oo] = terrains[ier, iier];
                        terrains[ier, iier].chunks[(2 - o) * 3 + 2 - oo] = terrain;
                    }
                }
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk terrain = worldChunks[i, ii];
                for (int o = 0; o < 3; o++)
                {
                    for (int oo = 0; oo < 3; oo++)
                    {
                        int ier = i - 1 + o;
                        int iier = ii - 1 + oo;
                        ier = ier >= worldChunkLoadSize ? 0 : ier < 0 ? worldChunkLoadSize - 1 : ier;
                        iier = iier >= worldChunkLoadSize ? 0 : iier < 0 ? worldChunkLoadSize - 1 : iier;
                        terrain.chunks[o * 3 + oo] = worldChunks[ier, iier];
                        worldChunks[ier, iier].chunks[(2 - o) * 3 + 2 - oo] = terrain;
                    }
                }
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk worldChunk = worldChunks[i, ii];
                for (int o = 0; o < worldChunkSize; o++)
                {
                    for (int oo = 0; oo < worldChunkSize; oo++)
                    {
                        worldChunk.AddTerrain(terrains[i * worldChunkSize + o, ii * worldChunkSize + oo], o, oo);
                    }
                }
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                heightsToLoad.Add(worldChunks[i, ii]);
            }
        }
    }
    
    //MeshData pooling

    ConcurrentBag<MeshData> meshDataPool = new ConcurrentBag<MeshData>();
    int meshDataLoaded = 0;

    void UnloadMeshData(WorldChunk wc)
    {
        wc.meshData.Clear();
    }

    public MeshData GetMeshData()
    {
        if (meshDataPool.IsEmpty)
        {
            meshDataLoaded++;
            return new MeshData();
        }
        else
        {
            MeshData md = null;
            while (!meshDataPool.TryTake(out md))
            {
                Thread.Sleep(1);
                if (meshDataPool.IsEmpty)
                {
                    meshDataLoaded++;
                    return new MeshData();
                }
            }
            return md;
        }
    }

    //Loading and Unloading Chunks

    void UnloadGame()
    {
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                int ier = GetIndex(i + chunkIndex.x);
                int iier = GetIndex(ii + chunkIndex.y);
                WorldChunk chunk = worldChunks[ier, iier];
                if (chunk.StructuresLoaded() == 9)
                {
                    chunk.SaveToDisk();
                }
            }
        }
    }

    void MoveChunks()
    {
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                int ier = GetIndex(i + chunkIndex.x);
                int iier = GetIndex(ii + chunkIndex.y);
                WorldChunk chunk = worldChunks[ier, iier];
                int index1 = chunk.index1;
                int index2 = chunk.index2;
                bool should = false;
                if (index1 != chunkIndex.x - worldChunkLoadSize / 2 + i | index2 != chunkIndex.y - worldChunkLoadSize / 2 + ii)
                {
                    index1 = chunkIndex.x - worldChunkLoadSize / 2 + i;
                    index2 = chunkIndex.y - worldChunkLoadSize / 2 + ii;
                    should = true;
                }
                if (should ? should : (new Vector2Int(index1, index2) - chunkIndex).magnitude > worldChunkLoadSize / 2)
                {
                    chunk.unloading = true;
                }
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                int ier = GetIndex(i+chunkIndex.x);
                int iier =  GetIndex(ii+chunkIndex.y);
                WorldChunk chunk = worldChunks[ier, iier];
                int index1 = chunk.index1;
                int index2 = chunk.index2;
                bool should = false;
                if (index1 != chunkIndex.x-worldChunkLoadSize/2+i | index2 != chunkIndex.y - worldChunkLoadSize / 2 + ii)
                {
                    index1 = chunkIndex.x - worldChunkLoadSize / 2 + i;
                    index2 = chunkIndex.y - worldChunkLoadSize / 2 + ii;
                    should = true;
                }
                if (should ? should : (new Vector2Int(index1, index2) - chunkIndex).magnitude > worldChunkLoadSize / 2)
                {
                    if (chunk.StructuresLoaded() == 9)
                    {
                        chunk.SaveToDisk();
                    }
                    if (chunk.graphics != null && chunk.graphics.Count > 0)
                    {
                        unloadGraphics.AddRange(chunk.graphics);
                        chunk.graphics.Clear();
                    }
                    chunk.Unload();
                    chunk.Load(index1, index2);
                }
            }
        }
        previousChunkIndex = chunkIndex;
        chunkMover = new Vector2Int();
    }
    
    Vector2Int currentPlayerPos = new Vector2Int();
    Vector2Int previousChunkMover = new Vector2Int(int.MaxValue, int.MaxValue);
    int threader = 0;
    bool MoveNeeded = false;

    //Main World Loading Thread

    void LoadTerrain()
    {

        threader = 0;
        currentPlayerPos = new Vector2Int(player.wp.posIndex.x, player.wp.posIndex.z);
        Vector2Int pos = currentPlayerPos + new Vector2Int((int)playerPos.x, (int)playerPos.z);
        WorldChunk.pos = pos;
        float mag = (playerVel.magnitude * 4 < 1 ? 1 : playerVel.magnitude * 4);
        mag = mag > worldChunkSizer ? worldChunkSizer : mag;
        Vector2Int playerDist = (new Vector2Int(pos.x, pos.y) - new Vector2Int(chunkIndex.x * worldChunkSizer, chunkIndex.y * worldChunkSizer));
        chunkMover = new Vector2Int(playerDist.x > 0 ? Mathf.FloorToInt((float)playerDist.x / worldChunkSizer) : Mathf.CeilToInt((float)playerDist.x / worldChunkSizer), playerDist.y > 0 ? Mathf.FloorToInt((float)playerDist.y / worldChunkSizer) : Mathf.CeilToInt((float)playerDist.y / worldChunkSizer));
        threader++;
        if (addGraphics && chunkMover.magnitude * worldChunkSizer > mag)
        {
            MoveNeeded = true;
            return;
        }
        else if (!addGraphics && chunkMover.magnitude * worldChunkSizer > mag)
        {
            chunkIndex += new Vector2Int(chunkMover.x, chunkMover.y);
            MoveChunks();
            voxelIndexPool.Sort((x, y) => y.CompareTo(x));
            WorldChunk.ReverseSort = true;
            reloadStructures.Sort();
            loadingGraphics.Sort();
            loadGraphics.Sort();
            graphicsGrass.Sort();
            structuresToLoad.Sort();
            worldsToLoad.Sort();
            WorldChunk.ReverseSort = false;
            for (int i = reloadStructures.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = reloadStructures[i];
                if (wc.unloading)
                {
                    reloadStructures.RemoveAt(i);
                }
            }
            for (int i = loadingGraphics.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = loadingGraphics[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    loadingGraphics.RemoveAt(i);
                }
            }
            
            for (int i = loadGraphics.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = loadGraphics[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    loadGraphics.RemoveAt(i);
                }
            }
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = graphicsGrass[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    graphicsGrass.RemoveAt(i);
                }
            }
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                }
            }
            for (int i = worldsToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = worldsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    worldsToLoad.RemoveAt(i);
                }
                else if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude <= worldChunkLoadSize / 2.0f - 2)
                {
                    loadingGraphics.Add(wc);
                    worldsToLoad.RemoveAt(i);
                }
            }
            player.SetWorldPos(playerPos);
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].PositionChunk(player.updateWp);
            }
            RepositionGraphics = true;
            MoveNeeded = false;
            addGraphics = true;
        }
        if (!AddGraphics)
        {
            threader++;
            heightsToLoad.Sort();
            threader++;
            int heightsLoaded = 0;
            for (int i = heightsToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = heightsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                }
                if (heightsLoaded >= 32)
                {
                    break;
                }
                if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude >= worldChunkLoadSize / 2.0f)
                {
                    continue;
                }
                if (!wc.saved && wc.LoadFromDisk())
                {
                    wc.Decompress();
                    wc.FinishChunk();
                    structuresToLoad.Add(wc);
                    heightsToLoad.RemoveAt(i);
                    heightsLoaded++;
                    continue;
                }
                if (!wc.AreHeightsLoaded())
                {
                    heightsLoaded++;
                    structuresToLoad.Add(wc);
                    heightsToLoad.RemoveAt(i);
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadHeights);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    else
                    {
                        while (loadingChunks.Count >= maxThreads - threadOffset)
                        {
                            for (int ii = 0; ii < loadingChunks.Count; ii++)
                            {
                                if (loadingChunks[ii].done)
                                {
                                    loadingChunks[ii].thread.Join();
                                    loadingChunks.RemoveAt(ii);
                                    ii--;
                                }
                            }
                        }
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadHeights);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                }
            }
            for (int ii = 0; ii < loadingChunks.Count; ii++)
            {
                loadingChunks[ii].thread.Join();
            }
            loadingChunks.Clear();
            reloadStructures.Sort();
            for (int i = reloadStructures.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = reloadStructures[i];
                if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude > worldChunkLoadSize / 2.0f - 1)
                {
                    continue;
                }
                if (!wc.CanLoadHeights() | !wc.CanLoadStructures())
                {
                    continue;
                }
                if (!wc.CanLoad())
                {
                    continue;
                }
                if (!wc.AreStructuresLoaded())
                {
                    wc.AddStructures();
                    wc.LoadStructures();
                    wc.structuresReloading = false;
                    reloadStructures.RemoveAt(i);
                }
            }
            structuresToLoad.Sort();
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude > worldChunkLoadSize / 2.0f-1)
                {
                    continue;
                }
                if (!wc.CanLoadHeights() | !wc.CanLoadStructures())
                {
                    continue;
                }
                if (!wc.CanLoad())
                {
                    continue;
                }
                if (!wc.AreStructuresLoaded())
                {
                    if (wc.allLoadedFromDisk())
                    {
                        wc.AddStructures();
                        wc.areStructuresLoaded = true;
                    }else
                    {
                        wc.AddStructures();
                        wc.LoadStructures();
                    }
                }
                if (wc.StructuresLoaded() == 9)
                {
                    worldsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                    wc.StructuresFinished = true;
                }
            }
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if (wc.StructuresLoaded() == 9)
                {
                    worldsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                    wc.StructuresFinished = true;
                }
            }
            AddGraphics = true;
        }
    }

    bool addGraphics;
    List<Thread> graphicsThreads = new List<Thread>();
    void RunLoadGraphics()
    {
        Vector2Int pos = currentPlayerPos + new Vector2Int((int)playerPos.x, (int)playerPos.z);
        if (AddGraphics)
        {
            if (worldsToLoad.Count > 0)
            {
                worldsToLoad.Sort();
                for (int i = worldsToLoad.Count - 1; i >= 0; i--)
                {
                    WorldChunk wc = worldsToLoad[i];
                    if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude > worldChunkLoadSize / 2.0f-2)
                    {
                        continue;
                    }
                    if (!wc.CanLoadHeights())
                    {
                        continue;
                    }
                    loadingGraphics.Add(wc);
                    worldsToLoad.RemoveAt(i);
                }
            }
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = graphicsGrass[i];
                wc.NeedsToLoad = false;

            }
            loadingGraphics.Sort();
            int c = 0;
            for (int i = loadingGraphics.Count - 1; i >= 0; i--)
            {
                if (c >= maxThreads)
                {
                    break;
                }
                WorldChunk wc = loadingGraphics[i];
                if (wc != null && wc.CanLoadHeights() && wc.StructuresLoaded() == 9)
                {
                    float dist = (new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude;
                    if (dist < GrassDistance)
                    {
                        if (wc.lodLevel != 0)
                        {
                            wc.MakeLoadable();
                            c++;
                        }
                    }
                    else if (dist < GrassDistance * 3)
                    {
                        if (wc.lodLevel != 1)
                        {
                            wc.MakeLoadable();
                            c++;
                        }
                    }
                    else if (dist < GrassDistance*6)
                    {
                        if (wc.lodLevel != 2)
                        {
                            wc.MakeLoadable();
                            c++;
                        }
                    }else
                    {
                        if (wc.lodLevel != 3)
                        {
                            wc.MakeLoadable();
                            c++;
                        }
                    }
                }
            }
            graphicsGrass.Sort();
            c = 0;
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = graphicsGrass[i];

                if (c >= maxThreads)
                {
                    break;
                }
                if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
                {
                    continue;
                }
                float dist = (new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude;
                if (dist < GrassDistance)
                {
                    if (wc.lodLevel != 0)
                    {
                        wc.MakeLoadable();
                        c++;
                    }
                }
                else if (dist < GrassDistance * 3)
                {
                    if (wc.lodLevel != 1)
                    {
                        wc.MakeLoadable();
                        c++;
                    }
                }
                else if (dist < GrassDistance * 6)
                {
                    if (wc.lodLevel != 2)
                    {
                        wc.MakeLoadable();
                        c++;
                    }
                }
                else
                {
                    if (wc.lodLevel != 3)
                    {
                        wc.MakeLoadable();
                        c++;
                    }
                }
            }
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = graphicsGrass[i];

                if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
                {
                    continue;
                }
                if (wc.GraphicsLoaded() == 9)
                {
                    if ((new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude > 256 + worldChunkSizer * 2)
                    {
                        if (!wc.compressed && !wc.NeedsToLoad)
                        {
                            wc.Compress();
                            wc.SaveToDisk();
                            continue;
                        }
                    }
                }
                if ((new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude > 256 + worldChunkSizer * 2)
                {
                }
                else
                {
                    if (wc.compressed)
                    {
                        wc.LoadFromDisk();
                        wc.Decompress();
                    }
                }
            }
            voxelIndexPool.Sort((x, y) => y.CompareTo(x));
            if (loadingGraphics.Count < 32)
            {
                AddGraphics = false;
            }
        }
        for (int i = 0; i < unloadMeshData.Count; i++)
        {
            MeshData md = unloadMeshData[i];
            md.Unload();
            meshDataPool.Add(md);
        }
        unloadMeshData.Clear();
        int amount = 0;
        loadingGraphics.Sort();
        for (int i = loadingGraphics.Count - 1; i >= 0; i--)
        {
            if (amount >= maxThreads)
            {
                break;
            }
            WorldChunk closest = loadingGraphics[i];
            if (closest != null && closest.CanLoadHeights() && closest.StructuresLoaded() == 9 && closest.CanLoad())
            {
                closest.meshData.Add(GetMeshData());
                closest.AddGraphics();
                float dist = (new Vector2Int(closest.index1 * worldChunkSizer, closest.index2 * worldChunkSizer) - pos).magnitude;
                if (dist < GrassDistance)
                {
                    closest.loading = true;
                    Thread thread = new Thread(closest.LoadGraphics);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    loadGraphics.Add(closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
                else if (dist < GrassDistance * 3)
                {
                    closest.loading = true;
                    Thread thread = new Thread(closest.LoadGraphicsNoGrass);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    loadGraphics.Add(closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
                else if (dist < GrassDistance*6)
                {
                    closest.loading = true;
                    Thread thread = new Thread(closest.LoadGraphicsSuperLowQ);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    loadGraphics.Add(closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }else
                {
                    closest.loading = true;
                    Thread thread = new Thread(closest.LoadGraphicsSuperSuperLowQ);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    loadGraphics.Add(closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
            }
        }
        for (int i = 0; i < graphicsThreads.Count; i++)
        {
            graphicsThreads[i].Join();
        }
        graphicsThreads.Clear();
        if (amount == 0)
        {
            AddGraphics = false;
        }
        amount = 0;
        graphicsGrass.Sort();
        for (int i = graphicsGrass.Count - 1; i >= 0; i--)
        {
            if (amount >= maxThreads)
            {
                break;
            }
            WorldChunk wc = graphicsGrass[i];

            if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
            {
                continue;
            }
            if (!wc.CanLoad())
            {
                continue;
            }
            float dist = (new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude;
            if (dist < GrassDistance)
            {
                if (wc.lodLevel != 0)
                {
                    wc.meshData.Add(GetMeshData());
                    wc.loading = true;
                    Thread thread = new Thread(wc.LoadGraphics);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    reloadGraphics.Add(wc);
                    amount++;
                }
            }
            else if (dist < GrassDistance * 3)
            {
                if (wc.lodLevel != 1)
                {
                    wc.meshData.Add(GetMeshData());
                    wc.loading = true;
                    Thread thread = new Thread(wc.LoadGraphicsNoGrass);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    reloadGraphics.Add(wc);
                    amount++;
                }
            }
            else if (dist < GrassDistance*6)
            {
                if (wc.lodLevel != 2)
                {
                    wc.meshData.Add(GetMeshData());
                    wc.loading = true;
                    Thread thread = new Thread(wc.LoadGraphicsSuperLowQ);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    reloadGraphics.Add(wc);
                    amount++;
                }
            }else
            {
                if (wc.lodLevel != 3)
                {
                    wc.meshData.Add(GetMeshData());
                    wc.loading = true;
                    Thread thread = new Thread(wc.LoadGraphicsSuperSuperLowQ);
                    thread.Start();
                    graphicsThreads.Add(thread);
                    reloadGraphics.Add(wc);
                    amount++;
                }
            }
        }
        for (int i = 0; i < graphicsThreads.Count; i++)
        {
            graphicsThreads[i].Join();
        }
        graphicsThreads.Clear();
        graphicsDone = true;
    }

    public static float GrassDistance = 256;

    List<WorldChunk> reloadGraphics = new List<WorldChunk>();

    //Actual Main World Thread Function

    public void LoadThread()
    {
        LoadTerrain();
        done = true;
    }

    //Returns index of the chunks based on Current position

    public int GetIndex(int i)
    {
        return i - Mathf.FloorToInt((float)i / worldChunkLoadSize) * worldChunkLoadSize;
    }
    
    List<Chunk> unloadChunkGraphics = new List<Chunk>();

    bool done = false;
    bool loading = false;
    Thread thread;
    Thread thread2;
    bool graphicsDone = false;
    bool graphicsLoading = false;
    Vector2Int chunkIndex = new Vector2Int();
    Vector2Int previousChunkIndex = new Vector2Int();
    Vector2Int chunkMover = new Vector2Int();
    Vector3 playerPos;
    Vector3 playerVel;

    List<Chunk> chunkPool = new List<Chunk>();
    List<Chunk> loadedGraphics = new List<Chunk>();

    Chunk GetChunk()
    {
        if (chunkPool.Count > 0)
        {
            Chunk chunk = chunkPool[chunkPool.Count - 1];
            chunkPool.RemoveAt(chunkPool.Count - 1);
            return chunk;
        }else
        {
            Chunk chunk = Instantiate(ChunkGraphics);
            loadedGraphics.Add(chunk);
            return chunk;
        }
    }
   
    List<WorldChunk> loadGraphics = new List<WorldChunk>();
    List<WorldChunk> graphicsGrass = new List<WorldChunk>();
    List<Chunk> unloadGraphics = new List<Chunk>();
    
    
    float _timer;
    float fpsAverage;
    int fpsCounter;

    List<MeshData> unloadMeshData = new List<MeshData>();
    bool LoadChunks = false;
    
    public bool paused = false;
    bool RepositionGraphics;
    bool AddGraphics = false;
    float garbageTimer = 0;

    void RunGame()
    {
        if (paused)
        {
            return;
        }
        garbageTimer += Time.deltaTime;
        if (garbageTimer >= 10)
        {
            //GarbageCollector.CollectIncremental(0);
            garbageTimer = 0;
        }
        int fpser = (int)(1f / Time.unscaledDeltaTime);
        fpsAverage += fpser;
        fpsCounter++;
        fps.text = "FPS: " + fpsAverage / fpsCounter;
        _timer = Time.unscaledTime + 1f;
        fpsAverage = 0;
        fpsCounter = 0;
        memory.text = "Memory: " + (System.GC.GetTotalMemory(false) / 1000000000.0);
        if (RepositionGraphics)
        {
            player.UpdateWorldPos();
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].SetPosition();
            }
            RepositionGraphics = false;
        }
        if (!loading)
        {
            playerPos = player.transform.position;
            playerPos.x = Mathf.Floor(playerPos.x);
            playerPos.y = Mathf.Floor(playerPos.y);
            playerPos.z = Mathf.Floor(playerPos.z);
            playerVel = player.velocity;
            //Debug.Log(meshDataLoaded + " " + meshDataPool.Count);
            thread = new Thread(LoadThread);
            thread.Start();
            loading = true;
        }
        else if (done)
        {
            thread.Join();
            position.text = "Position: ( " + (player.wp.posIndex.x) + " , " + (player.wp.posIndex.y) + " , " + (player.wp.posIndex.z) + " )";
            done = false;
            loading = false;
        }
        else
        {
            //Debug.Log(threader);
        }
        if (addGraphics)
        {
            if (LoadChunks && loadGraphics.Count == 0 && reloadGraphics.Count == 0)
            {
                LoadChunks = false;
            }
            if (!graphicsLoading && !LoadChunks)
            {
                if (MoveNeeded)
                {
                    addGraphics = false;
                    return;
                }
                graphicsLoading = true;
                thread2 = new Thread(RunLoadGraphics);
                thread2.Start();
            }else if (graphicsDone)
            {
                thread2.Join();
                graphicsLoading = false;
                graphicsDone = false;
                LoadChunks = true;
            }
            if (unloadGraphics.Count > 0)
            {
                int ind = unloadGraphics.Count-1;
                Chunk c = unloadGraphics[ind];
                unloadGraphics.RemoveAt(ind);
                c.Unload();
                chunkPool.Add(c);
            }
            if (LoadChunks && loadGraphics.Count > 0)
            {
                int ind = loadGraphics.Count-1;
                WorldChunk wc = loadGraphics[ind];
                Chunk c = GetChunk();
                c.load(worldChunkSizer, wc, 0);
                wc.graphics.Add(c);
                c.PositionChunk(player.wp);
                c.SetPosition();
                c.Reload();
                unloadMeshData.Add(wc.meshData[0]);
                wc.meshData.RemoveAt(0);
                if (wc.meshData.Count == 0)
                {
                    loadGraphics.RemoveAt(ind);
                    graphicsGrass.Add(wc);
                }
            }
            if (LoadChunks && reloadGraphics.Count > 0)
            {
                int ind = reloadGraphics.Count - 1;
                WorldChunk wc = reloadGraphics[ind];
                Chunk c = GetChunk();
                c.load(worldChunkSizer, wc, 0);
                c.PositionChunk(player.wp);
                c.SetPosition();
                unloadMeshData.Add(wc.meshData[0]);
                wc.meshData.RemoveAt(0);
                graphicsReloader.Add(c);
                if (wc.meshData.Count == 0)
                {
                    for (int i = 0; i < wc.graphics.Count; i++)
                    {
                        Chunk gc = wc.graphics[i];
                        gc.Unload();
                        chunkPool.Add(gc);
                    }
                    wc.graphics.Clear();
                    for (int i = 0; i < graphicsReloader.Count; i++)
                    {
                        Chunk gc = graphicsReloader[i];
                        gc.Reload();
                        wc.graphics.Add(gc);
                    }
                    graphicsReloader.Clear();
                    reloadGraphics.RemoveAt(ind);
                }
            }
        }
    }

    List<Chunk> graphicsReloader = new List<Chunk>();

    //Sliders
    public Slider LODSlider;
    public void SetDrawDistance()
    {
        Chunk.LODSize = LODSlider.value;
        Debug.Log(Chunk.LODSize);
        player.cam.farClipPlane = (float)Chunk.LODSize/60.0f * 1024.0f;
        for (int i = 0; i < loadedGraphics.Count; i++)
        {
            loadedGraphics[i].SetDrawDistance();
        }
    }

    public Slider GrassSlider;
    public void SetGrassDistance()
    {
        GrassDistance = GrassSlider.value * loadSize * size / 2.0f;
    }


    public void OnApplicationQuit()
    {
        Time.timeScale = 0;
        if (loading && thread.IsAlive)
        {
            thread.Join();
        }
        if (graphicsLoading && thread2.IsAlive)
        {
            thread2.Join();
        }
        UnloadGame();
        Chunk[] chunks = FindObjectsOfType<Chunk>();
        for (int i = 0; i < chunks.Length; i++)
        {
            Chunk c = chunks[i];
            c.Destroy();
        }
        world = null;
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Close();
            System.Diagnostics.Process.GetCurrentProcess().Kill();

        }
    }
    public void ExitGame()
    {
        Time.timeScale = 0;
        if (loading && thread.IsAlive)
        {
            thread.Join();
        }
        if (graphicsLoading && thread2.IsAlive)
        {
            thread2.Join();
        }
        UnloadGame();
        Chunk[] chunks = FindObjectsOfType<Chunk>();
        for (int i = 0; i < chunks.Length; i++)
        {
            Chunk c = chunks[i];
            c.Destroy();
        }
        world = null;
        Thread.Sleep(1000);
        MainMenu.LoadMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = paused ? false : true;
            if (paused)
            {
                pauseMenu.SetActive(true);
            }else
            {
                pauseMenu.SetActive(false);
            }
        }
        RunGame();

    }
}
