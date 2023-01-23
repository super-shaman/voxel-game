
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
    public TextMeshProUGUI amountOfChunks;
    public GameObject pauseMenu;
    public float LODSize;
    public int worldChunkSize = 8;
    byte size =  16;
    int loadSize = (MainMenu.loadSize*32/2 + 32/2);
    int worldChunkLoadSize;
    int worldChunkSizer;
    int maxThreads = System.Environment.ProcessorCount;
    int threadOffset = 0;

    TerrainChunk[,] terrains;
    WorldChunk[,] worldChunks;
    List<WorldChunk> heightsToLoad;
    List<WorldChunk> structuresToLoad;
    List<WorldChunk> worldsToLoad;
    List<WorldChunk> loadingGraphics;
    List<WorldChunk> loadingChunks;
    List<ChunkBatch> loadingBatches = new List<ChunkBatch>();
    List<ChunkBatch> unloadingBatches = new List<ChunkBatch>();

    public static World world;

    void Start()
    {
        Chunk.LODSize = LODSlider.value;
        Debug.Log(Chunk.LODSize);
        player.cam.farClipPlane = (float)Chunk.LODSize/60.0f * 1024.0f;
        RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
        WorldChunk.InitBuffer(size);
        BatchCache.LoadFull();
        if (!Directory.Exists("worldSave"))
        {
            Directory.CreateDirectory("worldSave");
        }
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
        Application.targetFrameRate = MainMenu.FrameRate;
        QualitySettings.vSyncCount = MainMenu.VSync ? 1 : 0;
    }

    //VoxelChunk Pooling
    
    VoxelChunkLinkedList voxelIndexPool = new VoxelChunkLinkedList();

    public void UnloadVoxelChunk(VoxelChunk chunk)
    {
        voxelIndexPool.Add(chunk);
    }

    public VoxelChunk GetVoxelChunk()
    {
        if (voxelIndexPool.Count() == 0)
        {
            VoxelChunk v = new VoxelChunk(size, 0, true);
            return v;
        }
        else
        {
            VoxelChunk chunk = voxelIndexPool.Head();
            voxelIndexPool.Remove(chunk);
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
                if (chunk.areGraphicsLoaded && !chunk.compressed)
                {
                    chunk.SaveToDisk();
                }
            }
        }
    }
    List<WorldChunk> updateChunks = new List<WorldChunk>();
    public void UpdateChunk(WorldChunk wc)
    {
        updateChunks.Add(wc);
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
                int index1 = chunk.newIndex1;
                int index2 = chunk.newIndex2;
                bool should = false;
                if (index1 != chunkIndex.x - worldChunkLoadSize / 2 + i | index2 != chunkIndex.y - worldChunkLoadSize / 2 + ii)
                {
                    if (chunk.unloading)
                    {
                        index1 = chunkIndex.x - worldChunkLoadSize / 2 + i;
                        index2 = chunkIndex.y - worldChunkLoadSize / 2 + ii;
                        chunk.SetIndexes(index1, index2);
                    }
                    else
                    {
                        should = true;
                    }
                }
                if (should ? should : (new Vector2Int(index1, index2) - chunkIndex).magnitude > worldChunkLoadSize / 2 && (index1 != chunkIndex.x - worldChunkLoadSize / 2 + i | index2 != chunkIndex.y - worldChunkLoadSize / 2 + ii | chunk.AreHeightsLoaded()))
                {
                    index1 = chunkIndex.x - worldChunkLoadSize / 2 + i;
                    index2 = chunkIndex.y - worldChunkLoadSize / 2 + ii;
                    chunk.SetIndexes(index1, index2);
                    if (chunk.unloading)
                    {
                    }else
                    {
                        chunk.unloading = true;
                    }
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
    bool LoadMoreChunks = false;
    bool BlockBreakNeeded = false;

    //Main World Loading Thread
    static int amountOfChunk = 0;
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
        if (BlockBreakNeeded && !LoadChunks)
        {
            byte br = player.BreakBlock();
            if (br != 2)
            {
                for (int i = 0; i < unloadMeshData.Count; i++)
                {
                    MeshData md = unloadMeshData[i];
                    md.Unload();
                    meshDataPool.Add(md);
                }
                unloadMeshData.Clear();
                for (int i = updateChunks.Count - 1; i >= 0; i--)
                {
                    WorldChunk closest = updateChunks[i];
                    if (closest != null)
                    {
                        closest.updating = false;
                        if (!closest.CanLoad())
                        {
                            closest.MakeLoadable();
                        }
                        closest.meshData.Add(GetMeshData());
                        float dist = (new Vector2Int(closest.index1 * worldChunkSizer, closest.index2 * worldChunkSizer) - pos).magnitude;
                        if (dist < GrassDistance)
                        {
                            if (loadingChunks.Count < maxThreads - threadOffset)
                            {
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphics);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
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
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphics);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
                            }
                            closest.loading = true;
                            updateGraphics.Add(closest);
                        }
                        else if (dist < GrassDistance * 2)
                        {
                            if (loadingChunks.Count < maxThreads - threadOffset)
                            {
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsNoGrass);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
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
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsNoGrass);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
                            }
                            closest.loading = true;
                            updateGraphics.Add(closest);
                        }
                        else if (dist < GrassDistance * 4)
                        {
                            if (loadingChunks.Count < maxThreads - threadOffset)
                            {
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsSuperLowQ);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
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
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsSuperLowQ);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
                            }
                            closest.loading = true;
                            updateGraphics.Add(closest);
                        }
                        else
                        {
                            if (loadingChunks.Count < maxThreads - threadOffset)
                            {
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsSuperSuperLowQ);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
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
                                closest.done = false;
                                closest.thread = new Thread(closest.LoadGraphicsSuperSuperLowQ);
                                closest.thread.Start();
                                loadingChunks.Add(closest);
                            }
                            closest.loading = true;
                            updateGraphics.Add(closest);
                        }
                    }
                }
                updateChunks.Clear();
                for (int ii = 0; ii < loadingChunks.Count; ii++)
                {
                    loadingChunks[ii].thread.Join();
                }
                loadingChunks.Clear();
                LoadMoreChunks = false;
                return;
            }
            LoadMoreChunks = true;
        }
        if (addGraphics && chunkMover.magnitude * worldChunkSizer > mag)
        {
            MoveNeeded = true;
        }
        else if (!addGraphics && chunkMover.magnitude * worldChunkSizer > mag)
        {
            chunkIndex += new Vector2Int(chunkMover.x, chunkMover.y);
            MoveChunks();
            WorldChunk.ReverseSort = 1;
            reloadStructures.Sort();
            loadingGraphics.Sort();
            loadGraphics.Sort();
            graphicsGrass.Sort();
            structuresToLoad.Sort();
            worldsToLoad.Sort();
            WorldChunk.ReverseSort = 0;
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
                    heightsToLoad.Add(wc);
                    loadingGraphics.RemoveAt(i);
                }
            }
            
            for (int i = loadGraphics.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = loadGraphics[i];
                if (wc.unloading)
                {
                    heightsToLoad.Add(wc);
                    loadGraphics.RemoveAt(i);
                }
            }
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = graphicsGrass[i];
                if (wc.unloading)
                {
                    heightsToLoad.Add(wc);
                    graphicsGrass.RemoveAt(i);
                }
            }
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if (wc.unloading)
                {
                    heightsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                }
            }
            for (int i = worldsToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = worldsToLoad[i];
                if (wc.unloading)
                {
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
            //visibleChunks.Clear();
            if (loadedGraphics.Count() > 0)
            {
                for (Chunk c = loadedGraphics.Head(); c != null;)
                {
                    if (c.Loaded())
                    {
                        c.PositionChunk(player.updateWp);
                    }
                    c = c.next;
                }
            }
            RepositionGraphics = true;
            MoveNeeded = false;
            graphicsDone = false;
            graphicsLoading = true;
            addGraphics = true;
        }
        if (!AddGraphics)
        {
            if (BlockBreakNeeded && !LoadMoreChunks)
            {
                AddGraphics = false;
                return;
            }
            if (voxelIndexPool.Count() > 50000)
            {
                for (int i = 0; i < 25000; i++)
                {
                    voxelIndexPool.Remove(voxelIndexPool.Head());
                }
            }
            threader++;
            WorldChunk.ReverseSort = 2;
            heightsToLoad.Sort();
            WorldChunk.ReverseSort = 0;
            threader++;
            int heightsLoaded = 0;
            int loadAmount = 32 < maxThreads * graphicsLoadSpeed ? maxThreads * graphicsLoadSpeed : 32;
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                WorldChunk wc = heightsToLoad[i];
                if (wc.unloading)
                {
                    bool add = false;
                    if (wc.updated && !wc.compressed && wc.SaveToDisk())
                    {
                        add = true;
                    }
                    if (wc.batched())
                    {
                        if (!unloadingBatches.Contains(wc.batch))
                        {
                            add = true;
                            unloadingBatches.Add(wc.batch);
                        }
                    }
                    if (wc.graphics != null && wc.graphics.Count > 0)
                    {
                        add = true;
                        for (int iii = 0; iii < wc.graphics.Count; iii++)
                        {
                            wc.graphics[iii].unloading = true;
                        }
                        unloadGraphics.AddRange(wc.graphics);
                        wc.graphics.Clear();
                    }
                    heightsLoaded++;
                    wc.Unload();
                    wc.FinishLoad();
                    wc.unloading = false;
                }
                if (heightsLoaded >= loadAmount)
                {
                    break;
                }
            }
            heightsToLoad.Sort();
            heightsLoaded = 0;
            for (int i = heightsToLoad.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                WorldChunk wc = heightsToLoad[i];
                if (heightsLoaded >= loadAmount)
                {
                    break;
                }
                if ((new Vector2Int(wc.newIndex1, wc.newIndex2) - chunkIndex).magnitude >= worldChunkLoadSize / 2.0f)
                {
                    continue;
                }
                if (wc.unloading)
                {
                    if (wc.areGraphicsLoaded && !wc.compressed && wc.updated)
                    {
                        wc.SaveToDisk();
                    }
                    if (wc.batched())
                    {
                        if (!unloadingBatches.Contains(wc.batch))
                        {
                            unloadingBatches.Add(wc.batch);
                        }
                    }
                    if (wc.graphics != null && wc.graphics.Count > 0)
                    {
                        for (int iii = 0; iii < wc.graphics.Count; iii++)
                        {
                            wc.graphics[iii].unloading = true;
                        }
                        unloadGraphics.AddRange(wc.graphics);
                        wc.graphics.Clear();
                    }
                    wc.Unload();
                    wc.FinishLoad();
                    wc.unloading = false;
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
            heightsLoaded = 0;
            for (int i = reloadStructures.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                if (heightsLoaded > loadAmount)
                {
                    break;
                }
                WorldChunk wc = reloadStructures[i];
                if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude > worldChunkLoadSize / 2.0f - 1)
                {
                    continue;
                }
                if (!wc.CanLoadHeights() | !wc.CanLoadStructures())
                {
                    continue;
                }
                if (!wc.AreStructuresLoaded())
                {
                    if (!wc.CanLoad())
                    {
                        wc.MakeLoadable();
                    }
                    if (wc.allLoadedFromDisk())
                    {
                        wc.AddStructures();
                        wc.areStructuresLoaded = true;
                    }
                    else
                    {
                        wc.AddStructures();
                        wc.LoadStructures();
                    }
                    wc.structuresReloading = false;
                    reloadStructures.RemoveAt(i);
                }
            }
            heightsLoaded = 0;
            structuresToLoad.Sort();
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                if (heightsLoaded > loadAmount)
                {
                    break;
                }
                WorldChunk wc = structuresToLoad[i];
                if ((new Vector2Int(wc.index1, wc.index2) - chunkIndex).magnitude > worldChunkLoadSize / 2.0f-1)
                {
                    continue;
                }
                if (!wc.CanLoadHeights() | !wc.CanLoadStructures())
                {
                    continue;
                }
                if (!wc.AreStructuresLoaded())
                {
                    if (!wc.CanLoad())
                    {
                        wc.MakeLoadable();
                    }
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
        if (BlockBreakNeeded && !LoadMoreChunks)
        {
            AddGraphics = false;
            return;
        }
        if (graphicsLoading && !graphicsDone)
        {
            RunLoadGraphics();
        }
    }

    bool addGraphics;
    List<Thread> graphicsThreads = new List<Thread>();

    public static int LODSwitchSpeed = Mathf.CeilToInt(0.5f*System.Environment.ProcessorCount);

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
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
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
                    else if (dist < GrassDistance * 2)
                    {
                        if (wc.lodLevel != 1)
                        {
                            wc.MakeLoadable();
                            c++;
                        }
                    }
                    else if (dist < GrassDistance*4)
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
                if (c >= maxThreads * graphicsLoadSpeed)
                {
                    break;
                }
            }
            graphicsGrass.Sort();
            c = 0;
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                WorldChunk wc = graphicsGrass[i];
                
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
                else if (dist < GrassDistance * 2)
                {
                    if (wc.lodLevel != 1)
                    {
                        wc.MakeLoadable();
                        c++;
                    }
                }
                else if (dist < GrassDistance * 4)
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
                if (c >= LODSwitchSpeed)
                {
                    break;
                }
            }
            if (BlockBreakNeeded && !LoadMoreChunks)
            {
                AddGraphics = false;
                return;
            }
            c = 0;
            int loadAmount = 32 < maxThreads * graphicsLoadSpeed ? maxThreads * graphicsLoadSpeed : 32;
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                if (c >= loadAmount+ LODSwitchSpeed)
                {
                    break;
                }
                WorldChunk wc = graphicsGrass[i];

                if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
                {
                    continue;
                }
                if (wc.GraphicsLoaded() == 9 && !wc.AnyNeedToLoad())
                {
                    if ((new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude > 256 + worldChunkSizer * 2)
                    {
                        if (!wc.compressed && !wc.NeedsToLoad)
                        {
                            c++;
                            wc.SaveToDisk();
                            wc.Compress();
                        }
                    }
                }
            }
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                WorldChunk wc = graphicsGrass[i];

                if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
                {
                    continue;
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
            c = 0;
            for (int i = graphicsGrass.Count - 1; i >= 0; i--)
            {
                if (BlockBreakNeeded && !LoadMoreChunks)
                {
                    AddGraphics = false;
                    return;
                }
                if (c >= 16)
                {
                    break;
                }
                WorldChunk wc = graphicsGrass[i];

                if (!(wc.CanLoadHeights() && wc.StructuresLoaded() == 9))
                {
                    continue;
                }
                if (wc.GraphicsLoaded() == 9)
                {
                    int batchSize = 3;
                    if (Batch && !wc.batched() && !wc.AnyNeedToLoad() && wc.index1 - Mathf.FloorToInt((float)(wc.index1/(double)batchSize)) * batchSize == 0 && wc.index2 - Mathf.FloorToInt((float)(wc.index2 / (double)batchSize)) * batchSize == 0)
                    {
                        ChunkBatch batch = new ChunkBatch();
                        bool Success = false;
                        if (batch.AddChunk(wc))
                        {
                            for (int o = 0; o < 3; o++)
                            {
                                for (int oo = 0; oo < 3; oo++)
                                {
                                    WorldChunk wwc = wc.chunks[o * 3 + oo];
                                    if (!(o == 1 && oo == 1) && !wwc.batched() && batch.AddChunk(wwc))
                                    {
                                        wc.batch = batch;
                                        wwc.batch = batch;
                                        Success = true;
                                    }
                                    /*if (batchSize > 3 && wwc.GraphicsLoaded() == 9)
                                    {
                                        for (int ooo = 0; ooo < 3; ooo++)
                                        {
                                            for (int oooo = 0; oooo < 3; oooo++)
                                            {
                                                WorldChunk wwwc = wwc.chunks[ooo * 3 + oooo];
                                                if (!(o - 1 + ooo == 1 && oo - 1 + oooo == 1) && !wwwc.batched() && batch.AddChunk(wwwc))
                                                {
                                                    wwwc.batch = batch;
                                                    wc.batch = batch;
                                                    Success = true;
                                                }
                                            }
                                        }
                                    }*/
                                }
                            }
                        }
                        if (Success)
                        {
                            c++;
                            loadingBatches.Add(batch);
                        }
                    }
                }
            }
            amountOfChunk = voxelIndexPool.Count();
            if (loadingGraphics.Count == 0)
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
        for (int i = loadingGraphics.Count - 1; i >= 0; i--)
        {
            if (BlockBreakNeeded && !LoadMoreChunks)
            {
                AddGraphics = false;
                return;
            }
            if (amount >= maxThreads* graphicsLoadSpeed)
            {
                break;
            }
            WorldChunk closest = loadingGraphics[i];
            if (closest != null && closest.CanLoadHeights() && closest.StructuresLoaded() == 9)
            {
                if (!closest.CanLoad())
                {
                    continue;
                }
                closest.meshData.Add(GetMeshData());
                closest.AddGraphics();
                float dist = (new Vector2Int(closest.index1 * worldChunkSizer, closest.index2 * worldChunkSizer) - pos).magnitude;
                if (dist < GrassDistance)
                {
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphics);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
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
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphics);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
                    }
                    closest.loading = true;
                    loadGraphics.Insert(0, closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
                else if (dist < GrassDistance * 2)
                {
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsNoGrass);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
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
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsNoGrass);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
                    }
                    closest.loading = true;
                    loadGraphics.Insert(0, closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
                else if (dist < GrassDistance*4)
                {
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsSuperLowQ);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
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
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsSuperLowQ);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
                    }
                    closest.loading = true;
                    loadGraphics.Insert(0, closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }else
                {
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsSuperSuperLowQ);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
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
                        closest.done = false;
                        closest.thread = new Thread(closest.LoadGraphicsSuperSuperLowQ);
                        closest.thread.Start();
                        loadingChunks.Add(closest);
                    }
                    closest.loading = true;
                    loadGraphics.Insert(0, closest);
                    loadingGraphics.RemoveAt(i);
                    amount++;
                }
            }
        }
        for (int ii = 0; ii < loadingChunks.Count; ii++)
        {
            loadingChunks[ii].thread.Join();
        }
        loadingChunks.Clear();
        if (amount == 0)
        {
            AddGraphics = false;
        }
        amount = 0;
        for (int i = graphicsGrass.Count - 1; i >= 0; i--)
        {
            if (BlockBreakNeeded && !LoadMoreChunks)
            {
                AddGraphics = false;
                return;
            }
            if (amount >= LODSwitchSpeed)
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
            if (graphicsThreads.Count >= maxThreads)
            {
                graphicsThreads[0].Join();
                graphicsThreads.RemoveAt(0);
            }
            float dist = (new Vector2Int(wc.index1 * worldChunkSizer, wc.index2 * worldChunkSizer) - pos).magnitude;
            if (dist < GrassDistance)
            {
                if (wc.lodLevel != 0)
                {
                    wc.meshData.Add(GetMeshData());
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadGraphics);
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
                        wc.thread = new Thread(wc.LoadGraphics);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    wc.loading = true;
                    reloadGraphics.Insert(0, wc);
                    amount++;
                }
            }
            else if (dist < GrassDistance * 2)
            {
                if (wc.lodLevel != 1)
                {
                    wc.meshData.Add(GetMeshData());
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadGraphicsNoGrass);
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
                        wc.thread = new Thread(wc.LoadGraphicsNoGrass);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    wc.loading = true;
                    reloadGraphics.Insert(0, wc);
                    amount++;
                }
            }
            else if (dist < GrassDistance*4)
            {
                if (wc.lodLevel != 2)
                {
                    wc.meshData.Add(GetMeshData());
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadGraphicsSuperLowQ);
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
                        wc.thread = new Thread(wc.LoadGraphicsSuperLowQ);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    wc.loading = true;
                    reloadGraphics.Insert(0,wc);
                    amount++;
                }
            }else
            {
                if (wc.lodLevel != 3)
                {
                    wc.meshData.Add(GetMeshData());
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadGraphicsSuperSuperLowQ);
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
                        wc.thread = new Thread(wc.LoadGraphicsSuperSuperLowQ);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    wc.loading = true;
                    reloadGraphics.Insert(0, wc);
                    amount++;
                }
            }
        }
        for (int ii = 0; ii < loadingChunks.Count; ii++)
        {
            loadingChunks[ii].thread.Join();
        }
        loadingChunks.Clear();
        graphicsDone = true;
    }

    public static bool Batch = true;
    public static float GrassDistance = 256;

    List<WorldChunk> reloadGraphics = new List<WorldChunk>();

    //Actual Main World Thread Function

    public void LoadThread()
    {
        LoadTerrain();
        for (int ii = 0; ii < loadingChunks.Count; ii++)
        {
            loadingChunks[ii].thread.Join();
        }
        loadingChunks.Clear();
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
    public Vector3 playerPos;
    Vector3 playerVel;

    //List<int> chunkPool = new List<int>();
    ChunkLinkedList loadedGraphics = new ChunkLinkedList();
    //List<Chunk> visibleChunks = new List<Chunk>();
    ChunkLinkedList chunkPool = new ChunkLinkedList();

    public void UnloadGraphics(Chunk chunk)
    {
        loadedGraphics.Remove(chunk);
        chunkPool.Add(chunk);
    }

    Chunk GetChunk()
    {
        if (chunkPool.Count() > 0)
        {
            Chunk c = chunkPool.Head();
            chunkPool.Remove(c);
            loadedGraphics.Add(c);
            /*if (!c.visible)
            {
                visibleChunks.Add(c);
                c.visible = true;
            }*/
            return c;
        }else
        {
            Chunk chunk = Instantiate(ChunkGraphics);
            loadedGraphics.Add(chunk);
            /*if (!chunk.visible)
            {
                visibleChunks.Add(chunk);
                chunk.visible = true;
            }*/
            return chunk;
        }
    }
   
    List<WorldChunk> loadGraphics = new List<WorldChunk>();
    List<WorldChunk> updateGraphics = new List<WorldChunk>();
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
    static MeshData BatchCache = new MeshData();
    void RunGame()
    {
        garbageTimer += Time.deltaTime;
        if (garbageTimer >= (collectorStrength == 0 ? 60 : collectorStrength == 1 ? 16 : collectorStrength == 2 ? 8 : collectorStrength == 3 ? 4 : 1))
        {
            if (collectorStrength != 0)
            {
                GarbageCollector.CollectIncremental((ulong)(1000000 * Time.deltaTime / 60.0f));
            }
            garbageTimer = 0;
        }
        if (paused)
        {
            return;
        }
        if (Input.GetMouseButton(0))
        {
            BlockBreakNeeded = true;
        }else
        {
            LoadMoreChunks = false;
            BlockBreakNeeded = false;
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
            if (loadedGraphics.Count() > 0)
            {
                for (Chunk c = loadedGraphics.Head(); c != null;)
                {
                    if (c.Loaded())
                    {
                        c.SetPosition();
                    }
                    c = c.next;
                }
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
            if (updateGraphics.Count > 0)
            {
                for (int i = 0; i < updateGraphics.Count; i++)
                {
                    WorldChunk wc = updateGraphics[i];
                    if (wc.batched())
                    {
                        ChunkBatch b = wc.batch;
                        if (b.batched)
                        {
                            b.chunk.Unload();
                            UnloadGraphics(b.chunk);
                            for (int ii = 0; ii < b.batchedChunks.Count; ii++)
                            {
                                WorldChunk wwc = b.batchedChunks[ii];
                                if (wwc.batch != null)
                                {
                                    if (wwc.graphics.Count > 0)
                                    {
                                        for (int iii = 0; iii < wwc.graphics.Count; iii++)
                                        {
                                            Chunk rc = wwc.graphics[iii];
                                            if (!rc.Loaded() && !rc.unloading)
                                            {
                                                rc.PositionChunk(player.wp);
                                                rc.SetPosition();
                                                rc.Reload();
                                            }
                                        }
                                    }
                                    wwc.batch = null;
                                }
                                else
                                {
                                }
                            }
                            b.batched = false;
                        }
                    }
                    for (int ii = 0; ii < wc.graphics.Count; ii++)
                    {
                        Chunk gc = wc.graphics[ii];
                        gc.Unload();
                        UnloadGraphics(gc);
                    }
                    wc.graphics.Clear();
                    for (int ii = 0; ii < wc.meshData.Count; ii++)
                    {
                        Chunk c = GetChunk();
                        c.load(worldChunkSizer, wc, ii);
                        wc.graphics.Add(c);
                        c.PositionChunk(player.wp);
                        c.SetPosition();
                        c.Reload();
                        unloadMeshData.Add(wc.meshData[ii]);
                    }
                    wc.meshData.Clear();
                }
                updateGraphics.Clear();
            }
            else
            {
            }
            position.text = "Position: ( " + (player.wp.posIndex.x) + " , " + (player.wp.posIndex.y) + " , " + (player.wp.posIndex.z) + " )";
            done = false;
            loading = false;
        }
        else
        {
            //Debug.Log(threader);
        }
        if (AddGraphics)
        {
            int unloadCount = 0;
            int maxCount = (unloadingBatches.Count > 1 ? 1 : Mathf.FloorToInt(unloadingBatches.Count));
            while (unloadCount < maxCount)
            {
                if (unloadingBatches.Count > 0)
                {
                    int ind = unloadingBatches.Count - 1;
                    ChunkBatch b = unloadingBatches[ind];
                    if (b.batched)
                    {
                        b.chunk.Unload();
                        UnloadGraphics(b.chunk);
                        for (int i = 0; i < b.batchedChunks.Count; i++)
                        {
                            WorldChunk wwc = b.batchedChunks[i];
                            if (wwc.batch != null)
                            {
                                if (wwc.graphics.Count > 0)
                                {
                                    for (int ii = 0; ii < wwc.graphics.Count; ii++)
                                    {
                                        Chunk rc = wwc.graphics[ii];
                                        if (!rc.Loaded() && !rc.unloading)
                                        {
                                            rc.PositionChunk(player.wp);
                                            rc.SetPosition();
                                            rc.Reload();
                                        }
                                    }
                                }
                                wwc.batch = null;
                            }
                            else
                            {
                            }
                        }
                        b.batched = false;
                    }
                    unloadCount++;
                    unloadingBatches.RemoveAt(ind);
                }
                else
                {
                    break;
                }
            }
            if (unloadGraphics.Count > 0)
            {
                unloadCount = 0;
                maxCount = (unloadGraphics.Count > 1 ? 1 : Mathf.FloorToInt(unloadGraphics.Count));
                while (unloadCount < maxCount)
                {
                    if (unloadGraphics.Count > 0)
                    {
                        int ind = unloadGraphics.Count - 1;
                        Chunk c = unloadGraphics[ind];
                        unloadGraphics.RemoveAt(ind);
                        unloadCount++;
                        c.Unload();
                        UnloadGraphics(c);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        if (addGraphics)
        {
            if (LoadChunks && loadGraphics.Count == 0 && reloadGraphics.Count == 0 && loadingBatches.Count == 0)
            {
                LoadChunks = false;
            }
            if (!graphicsLoading && !LoadChunks)
            {
                if (MoveNeeded | BlockBreakNeeded)
                {
                    addGraphics = false;
                    AddGraphics = false;
                    return;
                }
                graphicsLoading = true;
                //thread2 = new Thread(RunLoadGraphics);
                //thread2.Start();
            }else if (graphicsDone)
            {
                //thread2.Join();
                amountOfChunks.text = "" + amountOfChunk + " Chunks";
                graphicsLoading = false;
                graphicsDone = false;
                LoadChunks = true;
            }
            if (LoadChunks && loadGraphics.Count > 0)
            {
                int ind = loadGraphics.Count - 1;
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
                if (wc.batched())
                {
                    ChunkBatch b = wc.batch;
                    if (b.batched)
                    {
                        b.chunk.Unload();
                        UnloadGraphics(b.chunk);
                        for (int i = 0; i < b.batchedChunks.Count; i++)
                        {
                            WorldChunk wwc = b.batchedChunks[i];
                            if (wwc.batch != null)
                            {
                                if (wwc.graphics.Count > 0)
                                {
                                    for (int ii = 0; ii < wwc.graphics.Count; ii++)
                                    {
                                        Chunk rc = wwc.graphics[ii];
                                        if (!rc.Loaded() && !rc.unloading)
                                        {
                                            rc.PositionChunk(player.wp);
                                            rc.SetPosition();
                                            rc.Reload();
                                        }
                                    }
                                }
                                wwc.batch = null;
                            }
                            else
                            {
                            }
                        }
                        b.batched = false;
                    }
                }
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
                        UnloadGraphics(gc);
                    }
                    wc.graphics.Clear();
                    for (int i = 0; i < graphicsReloader.Count; i++)
                    {
                        Chunk gc = graphicsReloader[i];
                        gc.PositionChunk(player.wp);
                        gc.SetPosition();
                        gc.Reload();
                        wc.graphics.Add(gc);
                    }
                    graphicsReloader.Clear();
                    reloadGraphics.RemoveAt(ind);
                }
            }
            if (LoadChunks && loadingBatches.Count > 0)
            {
                int ind = loadingBatches.Count - 1;
                ChunkBatch batch = loadingBatches[ind];
                Chunk c = GetChunk();
                batch.chunk = c;
                batch.batched = true;
                WorldChunk wc = batch.batchedChunks[0];
                c.loadBare(worldChunkSizer, wc);
                MeshData md = GetMeshData();
                Chunk ch = wc.graphics[0];
                for (int i = 0; i < batch.batchedChunks.Count; i++)
                {
                    WorldChunk wwc = batch.batchedChunks[i];
                    Chunk chunk = wwc.graphics[0];
                    int index = md.vertices.Count;
                    int vertexCount = chunk.mf.mesh.vertexCount;
                    chunk.mf.mesh.GetVertices(BatchCache.vertices);
                    for (int ii = 0; ii < vertexCount; ii++)
                    {
                        md.vertices.Add(BatchCache.vertices[ii]+new Vector3((wwc.index1 - wc.index1) * worldChunkSizer, 0, (wwc.index2 - wc.index2) * worldChunkSizer) + (chunk.offset - ch.offset));
                    }
                    for (int iii = 0; iii < 9; iii++)
                    {
                        uint indexCount = chunk.mf.mesh.GetIndexCount(iii);
                        if (indexCount > 0)
                        {
                            int indexer = md.indices[iii].Count;
                            chunk.mf.mesh.GetIndices(BatchCache.indices[iii], iii);
                            for (int iiii = 0; iiii < indexCount; iiii++)
                            {
                                md.indices[iii].Add((ushort)(BatchCache.indices[iii][iiii]+index));
                            }
                        }
                    }
                    chunk.mf.mesh.GetNormals(BatchCache.normals);
                    for (int ii = 0; ii < vertexCount; ii++)
                    {
                        md.normals.Add(BatchCache.normals[ii]);
                    }
                    chunk.mf.mesh.GetUVs(0, BatchCache.uvs);
                    for (int ii = 0; ii < vertexCount; ii++)
                    {
                        md.uvs.Add(BatchCache.uvs[ii]);
                    }
                    chunk.Hide();
                    /*combine[i].mesh = chunk.mf.mesh;
                    combine[i].transform = Matrix4x4.Translate(new Vector3((wwc.index1 - wc.index1) * worldChunkSizer, 0, (wwc.index2 - wc.index2) * worldChunkSizer) + (chunk.offset - wc.graphics[0].offset));
                    chunk.gameObject.SetActive(false);*/
                }
                c.FinishBatch(worldChunkSizer, md);
                c.PositionChunk(player.wp);
                c.SetPosition();
                c.Reload();
                loadingBatches.RemoveAt(ind);
                unloadMeshData.Add(md);
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
        if (loadedGraphics.Count() > 0)
        {
            for (Chunk c = loadedGraphics.Head(); c != null;)
            {
                c.SetDrawDistance();
                c = c.next;
            }
        }
    }

    public Slider GrassSlider;
    public void SetGrassDistance()
    {
        GrassDistance = GrassSlider.value * 256;
    }

    public Slider collectorSlider;
    public static int collectorStrength = 0;
    public void SetCollectorStrength()
    {
        collectorStrength = (int)collectorSlider.value;
    }

    public Slider lodSwitchSpeed;
    public void SetLODSwitchSpeed()
    {
        LODSwitchSpeed = Mathf.CeilToInt(lodSwitchSpeed.value*maxThreads*0.5f);
    }

    public Slider recordingPerformance;
    public void SetRecordingPerformance()
    {
        maxThreads = System.Environment.ProcessorCount-(int)recordingPerformance.value;
        LODSwitchSpeed = Mathf.CeilToInt(lodSwitchSpeed.value * maxThreads * 0.5f);
    }

    public static int graphicsLoadSpeed = 1;
    public Slider graphicsLoadSpeedSlider;
    public void SetGraphicsLoadSpeed()
    {
        graphicsLoadSpeed = (int)graphicsLoadSpeedSlider.value;
    }

    public void OnApplicationQuit()
    {
        Time.timeScale = 0;
        if (loading && thread.IsAlive)
        {
            thread.Join();
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

        }
    }

    public void ExitGame()
    {
        Time.timeScale = 0;
        if (loading && thread.IsAlive)
        {
            thread.Join();
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
