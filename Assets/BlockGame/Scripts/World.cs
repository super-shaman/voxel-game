using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEditor;
public class World : MonoBehaviour
{

    public Chunk ChunkGraphics;
    public Player player;
    public TextMeshProUGUI fps;
    public TextMeshProUGUI memory;
    public TextMeshProUGUI position;
    public GameObject pauseMenu;

    void Start()
    {
        if (!Application.isEditor)
        {
            GarbageCollector.incrementalTimeSliceNanoseconds = 4000000;
        }
        LoadLoadOrder2();
        LoadWorld();
    }

    void LoadLoadOrder()
    {

        VoxelChunk.loadOrder = new Vector3Int[size * size * size];
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                for (int iii = 0; iii < size; iii++)
                {
                    VoxelChunk.loadOrder[i * size * size + ii * size + iii] = new Vector3Int(i, ii, iii);
                }
            }
        }
        for (int i = 0; i < VoxelChunk.loadOrder.Length; i++)
        {
            Vector3Int wc = VoxelChunk.loadOrder[i];
            for (int ii = i; ii < VoxelChunk.loadOrder.Length; ii++)
            {
                Vector3Int wc2 = VoxelChunk.loadOrder[ii];
                if (wc.magnitude > wc2.magnitude)
                {
                    VoxelChunk.loadOrder[i] = wc2;
                    VoxelChunk.loadOrder[ii] = wc;
                    wc = wc2;
                }
            }
        }
        VoxelChunk.loadOrderReverse = new Vector3Int[size * size * size];
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                for (int iii = 0; iii < size; iii++)
                {
                    VoxelChunk.loadOrderReverse[i * size * size + ii * size + iii] = new Vector3Int(i, ii, iii);
                }
            }
        }
        for (int i = 0; i < VoxelChunk.loadOrderReverse.Length; i++)
        {
            Vector3Int wc = VoxelChunk.loadOrderReverse[i];
            for (int ii = i; ii < VoxelChunk.loadOrderReverse.Length; ii++)
            {
                Vector3Int wc2 = VoxelChunk.loadOrderReverse[ii];
                if (wc.magnitude < wc2.magnitude)
                {
                    VoxelChunk.loadOrderReverse[i] = wc2;
                    VoxelChunk.loadOrderReverse[ii] = wc;
                    wc = wc2;
                }
            }
        }
        WorldChunk.loadOrder = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrder[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        for (int i = 0; i < WorldChunk.loadOrder.Length; i++)
        {
            Vector2Int wc = WorldChunk.loadOrder[i];
            for (int ii = i; ii < WorldChunk.loadOrder.Length; ii++)
            {
                Vector2Int wc2 = WorldChunk.loadOrder[ii];
                if (wc.magnitude > wc2.magnitude)
                {
                    WorldChunk.loadOrder[i] = wc2;
                    WorldChunk.loadOrder[ii] = wc;
                    wc = wc2;
                }
            }
        }
        WorldChunk.loadOrderReverse = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrderReverse[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        for (int i = 0; i < WorldChunk.loadOrderReverse.Length; i++)
        {
            Vector2Int wc = WorldChunk.loadOrderReverse[i];
            for (int ii = i; ii < WorldChunk.loadOrderReverse.Length; ii++)
            {
                Vector2Int wc2 = WorldChunk.loadOrderReverse[ii];
                if (wc.magnitude < wc2.magnitude)
                {
                    WorldChunk.loadOrderReverse[i] = wc2;
                    WorldChunk.loadOrderReverse[ii] = wc;
                    wc = wc2;
                }
            }
        }
    }

    void LoadLoadOrder2()
    {

        Vector2Int[]  loaderderer = new Vector2Int[size * size];
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                loaderderer[i * size + ii] = new Vector2Int(i, ii);
            }
        }
        for (int i = 0; i < loaderderer.Length; i++)
        {
            Vector2Int wc = loaderderer[i];
            for (int ii = i; ii < loaderderer.Length; ii++)
            {
                Vector2Int wc2 = loaderderer[ii];
                if (wc.magnitude > wc2.magnitude)
                {
                    loaderderer[i] = wc2;
                    loaderderer[ii] = wc;
                    wc = wc2;
                }
            }
        }
        VoxelChunk.loadOrder = new Vector3Int[size * size * size];
        for (int iii = 0; iii < size; iii++)
        {
            for (int i = 0; i < loaderderer.Length; i++)
            {
                Vector2Int v = loaderderer[i];
                VoxelChunk.loadOrder[v.x * size * size + v.y * size + iii] = new Vector3Int(v.x, v.y, iii);
            }
        }
        VoxelChunk.loadOrderReverse = new Vector3Int[size * size * size];
        for (int iii = size-1; iii >= 0; iii--)
        {
            for (int i = loaderderer.Length-1; i >= 0; i--)
            {
                Vector2Int v = loaderderer[i];
                VoxelChunk.loadOrderReverse[v.x * size * size + v.y * size + iii] = new Vector3Int(v.x, v.y, iii);
            }
        }
        WorldChunk.loadOrder = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrder[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        for (int i = 0; i < WorldChunk.loadOrder.Length; i++)
        {
            Vector2Int wc = WorldChunk.loadOrder[i];
            for (int ii = i; ii < WorldChunk.loadOrder.Length; ii++)
            {
                Vector2Int wc2 = WorldChunk.loadOrder[ii];
                if (wc.magnitude > wc2.magnitude)
                {
                    WorldChunk.loadOrder[i] = wc2;
                    WorldChunk.loadOrder[ii] = wc;
                    wc = wc2;
                }
            }
        }
        WorldChunk.loadOrderReverse = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrderReverse[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        for (int i = 0; i < WorldChunk.loadOrderReverse.Length; i++)
        {
            Vector2Int wc = WorldChunk.loadOrderReverse[i];
            for (int ii = i; ii < WorldChunk.loadOrderReverse.Length; ii++)
            {
                Vector2Int wc2 = WorldChunk.loadOrderReverse[ii];
                if (wc.magnitude < wc2.magnitude)
                {
                    WorldChunk.loadOrderReverse[i] = wc2;
                    WorldChunk.loadOrderReverse[ii] = wc;
                    wc = wc2;
                }
            }
        }
    }

    int size = 8;
    int loadSize = (256+16);//64+32+2;
    public int worldChunkSize = 1;
    int worldChunkLoadSize;
    int worldChunkSizer;

    TerrainChunk[,] terrains;
    WorldChunk[,] worldChunks;

    public static World world;
    List<TerrainChunk> terrainPool = new List<TerrainChunk>();

    public void UnloadTerrain(TerrainChunk terrain)
    {

    }

    ConcurrentBag<VoxelChunk> voxelPool = new ConcurrentBag<VoxelChunk>();

    public void UnloadVoxelChunk(VoxelChunk chunk)
    {
        voxelPool.Add(chunk);
    }

    public VoxelChunk GetVoxelChunk()
    {
        if (voxelPool.Count == 0)
        {
            return new VoxelChunk(size);
        }else
        {
            VoxelChunk chunk;
            while(!voxelPool.TryTake(out chunk))
            {

            }
            return chunk;
        }
    }

    public void LoadWorld()
    {
        world = this;
        worldChunkLoadSize = loadSize / worldChunkSize;
        worldChunkSizer = worldChunkSize * size;
        worldChunks = new WorldChunk[worldChunkLoadSize, worldChunkLoadSize];
        terrains = new TerrainChunk[loadSize, loadSize];
        Init();
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                heightsToLoad.Add(worldChunks[i, ii]);
            }
        }
    }

    void Init()
    {
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk worldChunk = new WorldChunk(worldChunkSize, -worldChunkLoadSize / 2 + i, -worldChunkLoadSize / 2 + ii);
                worldChunks[i, ii] = worldChunk;
            }
        }
        for (int i = 0; i < loadSize; i++)
        {
            for (int ii = 0; ii < loadSize; ii++)
            {
                TerrainChunk terrain = new TerrainChunk(size, 1024 / size, 1024 / size);
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
                        worldChunk.AddTerrain(terrains[i * worldChunkSize + o, ii * worldChunkSize + oo],o,oo);
                    }
                }
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                worldChunks[i, ii].Load();
            }
        }
    }
    
    List<WorldChunk> heightsToLoad = new List<WorldChunk>();
    List<WorldChunk> chunksToLoad = new List<WorldChunk>();
    List<WorldChunk> structuresToLoad = new List<WorldChunk>();
    List<WorldChunk> graphicsToLoad = new List<WorldChunk>();
    List<WorldChunk> worldsToLoad = new List<WorldChunk>();
    List<WorldChunk> unloadChunks = new List<WorldChunk>();
    int maxThreads = System.Environment.ProcessorCount;
    int threadOffset = 2;
    List<WorldChunk> loadingChunks = new List<WorldChunk>();

    void MoveChunks()
    {
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                int ier = GetIndex(i);
                int iier = GetIndex(ii);
                WorldChunk chunk = worldChunks[i, ii];
                int index1 = chunk.index1;
                int index2 = chunk.index2;
                Vector2Int v = new Vector2Int(index1 - chunkIndex.x > worldChunkLoadSize / 2 - 1 ? -worldChunkLoadSize : index1 - chunkIndex.x < -worldChunkLoadSize / 2 ? worldChunkLoadSize : 0,
                    index2 - chunkIndex.y > worldChunkLoadSize / 2 - 1 ? -worldChunkLoadSize : index2 - chunkIndex.y < -worldChunkLoadSize / 2 ? worldChunkLoadSize : 0);
                bool should = false;
                while (v.magnitude > 0)
                {
                    index1 += v.x;
                    index2 += v.y;
                    v = new Vector2Int(index1 - chunkIndex.x > worldChunkLoadSize / 2 - 1 ? -worldChunkLoadSize : index1 - chunkIndex.x < -worldChunkLoadSize / 2 ? worldChunkLoadSize : 0,
                    index2 - chunkIndex.y > worldChunkLoadSize / 2 - 1 ? -worldChunkLoadSize : index2 - chunkIndex.y < -worldChunkLoadSize / 2 ? worldChunkLoadSize : 0);
                    should = true;
                }
                if (should)
                {
                    if (chunk.graphics != null)
                    {
                        unloadChunkGraphics.AddRange(chunk.graphics);
                        chunk.graphics.Clear();
                    }
                    chunk.Unload();
                    unloadChunks.Add(chunk);
                    chunk.Load(index1, index2);
                    chunk.Load();
                }
            }
        }
        previousChunkIndex = chunkIndex;
        chunkMover = new Vector2Int();
    }
    
    Vector2Int currentPlayerPos = new Vector2Int();
    List<MeshData> meshDataPool = new List<MeshData>();

    void UnloadMeshData(WorldChunk wc)
    {
        wc.meshData.Clear();
    }

    int meshDataLoaded = 0;

    public MeshData GetMeshData()
    {
        if (meshDataPool.Count == 0)
        {
            meshDataLoaded++;
            return new MeshData();
        }
        else
        {
            int index = meshDataPool.Count - 1;
            MeshData md = meshDataPool[index];
            meshDataPool.RemoveAt(index);
            return md;
        }
    }

    Vector2Int previousChunkMover = new Vector2Int(int.MaxValue, int.MaxValue);
    void LoadTerrain()
    {
        for (int i = 0; i < unloadMeshData.Count; i++)
        {
            MeshData md = unloadMeshData[i];
            md.Unload();
            meshDataPool.Add(md);
        }
        unloadMeshData.Clear();
        float mag = (playerVel.magnitude * 2 < 1 ? 1 : playerVel.magnitude * 2);
        if (playerPos.magnitude > mag)
        {
            player.SetWorldPos(playerPos);
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].PositionChunk(player.wp);
            }
            RepositionGraphics = true;
        }
        currentPlayerPos = new Vector2Int(player.wp.posIndex.x, player.wp.posIndex.z);
        Vector2Int pos = currentPlayerPos;
        if (!UnloadingGraphics && playerPos.magnitude > mag)
        {
            Vector2Int playerDist = (new Vector2Int(pos.x, pos.y) - new Vector2Int(chunkIndex.x * worldChunkSizer, chunkIndex.y * worldChunkSizer));
            chunkMover = new Vector2Int(playerDist.x > 0 ? Mathf.FloorToInt(playerDist.x / worldChunkSizer) : Mathf.CeilToInt(playerDist.x / worldChunkSizer), playerDist.x > 0 ? Mathf.FloorToInt(playerDist.y / worldChunkSizer) : Mathf.CeilToInt(playerDist.y / worldChunkSizer));
        }
        if (previousChunkMover != chunkMover)
        {
            previousChunkMover = chunkMover;
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                WorldChunk wc = heightsToLoad[i];
                for (int ii = i; ii < heightsToLoad.Count; ii++)
                {
                    WorldChunk wc2 = heightsToLoad[ii];
                    if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (new Vector2(wc2.index1 * size * worldChunkSize, wc2.index2 * size * worldChunkSize) - pos).magnitude)
                    {
                        heightsToLoad[i] = wc2;
                        heightsToLoad[ii] = wc;
                        wc = wc2;
                    }
                }
            }
            for (int i = 0; i < worldsToLoad.Count; i++)
            {
                WorldChunk wc = worldsToLoad[i];
                for (int ii = i; ii < worldsToLoad.Count; ii++)
                {
                    WorldChunk wc2 = worldsToLoad[ii];
                    if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (new Vector2(wc2.index1 * size * worldChunkSize, wc2.index2 * size * worldChunkSize) - pos).magnitude)
                    {
                        worldsToLoad[i] = wc2;
                        worldsToLoad[ii] = wc;
                        wc = wc2;
                    }
                }
            }
        }
        /*if (!SortGraphics && chunkMover.magnitude > 0)
        {
            RemoveGraphics = false;
            StopLoadingGraphics = true;
            while (StopLoadingGraphics)
            {
                Thread.Sleep((int)(1.0f / 60.0f * 1000));
                if (RemoveGraphics)
                {
                    break;
                }
            }
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].Sort();
            }
            SortGraphics = true;
            while (SortGraphics)
            {
                Thread.Sleep((int)(1.0f / 60.0f * 1000));
            }
            StopLoadingGraphics = false;
            RemoveGraphics = false;
        }*/
        if (reloadChunks)
        {
            for (int i = 0; i < dontLoadCurrent.Count; i++)
            {
                if (dontLoadCurrent[i].unloading)
                {
                    heightsToLoad.Add(dontLoadCurrent[i]);
                    dontLoadCurrent.RemoveAt(i);
                    i--;
                }
            }
            reloadChunks = false;
        }
        if (!LoadingGraphics && meshDataPool.Count == meshDataLoaded)
        {
            /*for (int i = 0; i < worldsToLoad.Count; i++)
            {
                WorldChunk wc = worldsToLoad[i];
                for (int ii = i; ii < worldsToLoad.Count; ii++)
                {
                    WorldChunk wc2 = worldsToLoad[ii];
                    if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (new Vector2(wc2.index1 * size * worldChunkSize, wc2.index2 * size * worldChunkSize) - pos).magnitude)
                    {
                        worldsToLoad[i] = wc2;
                        worldsToLoad[ii] = wc;
                        wc = wc2;
                    }
                }
            }
            int chunkLoadAmount = 8;
            int amountLoaded = 0;
            for (int i = 0; i < worldsToLoad.Count; i++)
            {
                if (amountLoaded >= chunkLoadAmount | !(meshDataLoaded-meshDataPool.Count < chunkLoadAmount))
                {
                    break;
                }
                amountLoaded++;
                WorldChunk wc = worldsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    worldsToLoad.RemoveAt(i);
                    i--;
                    heightsToLoad.Add(wc);
                    amountLoaded--;
                    continue;
                }
                if (loadingChunks.Count < maxThreads - threadOffset)
                {
                    wc.meshData.Add(GetMeshData());
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
                    wc.meshData.Add(GetMeshData());
                    wc.done = false;
                    wc.thread = new Thread(wc.LoadGraphics);
                    wc.thread.Start();
                    loadingChunks.Add(wc);
                }
                graphicsToLoad.Add(wc);
                worldsToLoad.RemoveAt(i);
                i--;
            }
            for (int ii = 0; ii < loadingChunks.Count; ii++)
            {
                loadingChunks[ii].thread.Join();
            }
            loadingChunks.Clear();*/
            if (chunkMover.magnitude <= 1)
            {
                WorldChunk closest = null;
                int index = 0;
                for (int i = 0; i < worldsToLoad.Count; i++)
                {
                    WorldChunk wc = worldsToLoad[i];
                    if (wc.unloading)
                    {
                        wc.unloading = false;
                        worldsToLoad.RemoveAt(i);
                        i--;
                        heightsToLoad.Add(wc);
                        continue;
                    }
                    if (closest == null)
                    {
                        closest = wc;
                        index = i;
                    }
                    else if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude < (new Vector2(closest.index1 * size * worldChunkSize, closest.index2 * size * worldChunkSize) - pos).magnitude)
                    {
                        closest = wc;
                        index = i;
                    }
                }
                if (closest != null)
                {
                    closest.meshData.Add(GetMeshData());
                    closest.LoadGraphics();
                    graphicsToLoad.Add(closest);
                    worldsToLoad.RemoveAt(index);
                }
                LoadingGraphics = true;
                if (graphicsToLoad.Count > 0)
                {
                    return;
                }
            }
        }
        if (!UnloadingGraphics)
        {
            if (chunkMover.magnitude > 0)
            {
                chunkIndex += new Vector2Int(chunkMover.x, chunkMover.y);
            }
            MoveChunks();
            UnloadingGraphics = true;
            int heightsLoaded = 0;
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                WorldChunk wc = heightsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                }
                /*if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (worldChunkLoadSize+4) * worldChunkSize * size / 2.0f)
                {
                    continue;
                }*/
                if (heightsLoaded >= 25)
                {
                    break;
                }
                if (!wc.AreHeightsLoaded())
                {
                    wc.AddHeights();
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
                        heightsLoaded++;
                    }
                    if (wc.HeightsLoaded() == 9)
                    {
                        chunksToLoad.Add(wc);
                        heightsToLoad.RemoveAt(i);
                        i--;
                    }
                }
                else if (wc.HeightsLoaded() == 9)
                {
                    chunksToLoad.Add(wc);
                    heightsToLoad.RemoveAt(i);
                    i--;
                }
            }
            for (int ii = 0; ii < loadingChunks.Count; ii++)
            {
                loadingChunks[ii].thread.Join();
            }
            loadingChunks.Clear();
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                WorldChunk wc = heightsToLoad[i];
                if (wc.HeightsLoaded() == 9)
                {
                    chunksToLoad.Add(wc);
                    heightsToLoad.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < chunksToLoad.Count; i++)
            {
                WorldChunk wc = chunksToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    chunksToLoad.RemoveAt(i);
                    heightsToLoad.Add(wc);
                    continue;
                }
                if (!(wc.index1 - chunkIndex.x >= -worldChunkLoadSize / 2 + 1 && wc.index1 - chunkIndex.x < worldChunkLoadSize / 2 - 2 && wc.index2 - chunkIndex.y >= -worldChunkLoadSize / 2 + 1 && wc.index2 - chunkIndex.y < worldChunkLoadSize / 2 - 2))
                {
                    continue;
                }
                if (!wc.AreChunksLoaded())
                {
                    wc.AddChunks();
                    wc.LoadVoxelChunks();
                    if (loadingChunks.Count < maxThreads - threadOffset)
                    {
                        wc.done = false;
                        wc.thread = new Thread(wc.LoadChunks);
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
                        wc.thread = new Thread(wc.LoadChunks);
                        wc.thread.Start();
                        loadingChunks.Add(wc);
                    }
                    if (wc.ChunksLoaded() == 9)
                    {
                        structuresToLoad.Add(wc);
                        chunksToLoad.RemoveAt(i);
                        i--;
                    }
                }
                else if (wc.ChunksLoaded() == 9)
                {
                    structuresToLoad.Add(wc);
                    chunksToLoad.RemoveAt(i);
                    i--;
                }
            }
            for (int ii = 0; ii < loadingChunks.Count; ii++)
            {
                loadingChunks[ii].thread.Join();
            }
            loadingChunks.Clear();
            for (int i = 0; i < chunksToLoad.Count; i++)
            {
                WorldChunk wc = chunksToLoad[i];
                if (wc.ChunksLoaded() == 9)
                {
                    structuresToLoad.Add(wc);
                    chunksToLoad.RemoveAt(i);
                    i--;
                }
            }
            bool finished = false;
            while (!finished)
            {
                int structuresLoaded = 0;
                for (int i = 0; i < structuresToLoad.Count; i++)
                {
                    WorldChunk wc = structuresToLoad[i];
                    if (wc.unloading)
                    {
                        wc.unloading = false;
                        structuresToLoad.RemoveAt(i);
                        heightsToLoad.Add(wc);
                        continue;
                    }
                    if (!(wc.index1 - chunkIndex.x >= -worldChunkLoadSize / 2 + 2 && wc.index1 - chunkIndex.x < worldChunkLoadSize / 2 - 3 && wc.index2 - chunkIndex.y >= -worldChunkLoadSize / 2 + 2 && wc.index2 - chunkIndex.y < worldChunkLoadSize / 2 - 3))
                    {
                        continue;
                    }
                    if (!wc.AreStructuresLoaded() && !wc.AreStructuresLoading())
                    {
                        wc.AddStructures();
                        structuresLoaded++;
                        if (loadingChunks.Count < maxThreads - threadOffset)
                        {
                            wc.done = false;
                            wc.loading = true;
                            wc.thread = new Thread(wc.LoadStructures);
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
                                        loadingChunks[ii].loading = false;
                                        loadingChunks.RemoveAt(ii);
                                        ii--;
                                    }
                                }
                            }
                            wc.done = false;
                            wc.loading = true;
                            wc.thread = new Thread(wc.LoadStructures);
                            wc.thread.Start();
                            loadingChunks.Add(wc);
                        }
                        if (wc.StructuresLoaded() == 9)
                        {
                            worldsToLoad.Add(wc);
                            structuresToLoad.RemoveAt(i);
                            i--;
                        }
                    }
                    else if (wc.StructuresLoaded() == 9)
                    {
                        worldsToLoad.Add(wc);
                        structuresToLoad.RemoveAt(i);
                        i--;
                    }
                }
                for (int ii = 0; ii < loadingChunks.Count; ii++)
                {
                    loadingChunks[ii].thread.Join();
                    loadingChunks[ii].loading = false;
                }
                loadingChunks.Clear();
                for (int i = 0; i < structuresToLoad.Count; i++)
                {
                    WorldChunk wc = structuresToLoad[i];
                    if (wc.StructuresLoaded() == 9)
                    {
                        worldsToLoad.Add(wc);
                        structuresToLoad.RemoveAt(i);
                        i--;
                    }
                }
                if (structuresLoaded == 0)
                {
                    finished = true;
                }
            }
        }
    }

    public void LoadThread()
    {
        LoadTerrain();
        done = true;
    }

    public int GetIndex(int i)
    {
        return i - Mathf.FloorToInt((float)i / worldChunkLoadSize) * worldChunkLoadSize;
    }
    
    List<Chunk> unloadChunkGraphics = new List<Chunk>();

    bool done = false;
    bool loading = false;
    Thread thread;
    Vector2Int chunkIndex = new Vector2Int();
    Vector2Int previousChunkIndex = new Vector2Int();
    Vector2Int chunkMover = new Vector2Int();
    Vector3 playerPos;
    Vector3 playerVel;
    List<Chunk> chunkPool = new List<Chunk>();
    List<Chunk> loadedGraphics = new List<Chunk>();
    bool SortGraphics;


    Chunk GetChunk()
    {
        if (chunkPool.Count > 0)
        {
            Chunk chunk = chunkPool[chunkPool.Count - 1];
            chunkPool.RemoveAt(chunkPool.Count - 1);
            chunk.Reload();
            return chunk;
        }else
        {
            return Instantiate(ChunkGraphics);
        }
    }
   
    List<WorldChunk> loadGraphics = new List<WorldChunk>();
    bool reloadChunks = false;
    List<WorldChunk> dontLoad = new List<WorldChunk>();
    List<WorldChunk> dontLoadCurrent = new List<WorldChunk>();
    List<Chunk> unloadGraphics = new List<Chunk>();
    

    bool LoadingGraphics = false;
    bool UnloadingGraphics = false;
    float _timer;
    float fpsAverage;
    int fpsCounter;

    List<MeshData> unloadMeshData = new List<MeshData>();
    bool LoadChunks = false;
    
    IEnumerator LoadGraphics()
    {
        while (loadGraphics.Count > 0)
        {
            WorldChunk wc = loadGraphics[0];
            if (!wc.unloading)
            {
                Chunk c = GetChunk();
                c.load(worldChunkSizer, wc, 0);
                c.PositionChunk(player.wp);
                c.SetPosition();
                loadedGraphics.Add(c);
                unloadMeshData.Add(wc.meshData[0]);
                wc.meshData.RemoveAt(0);
            }
            if (wc.meshData.Count == 0)
            {
                loadGraphics.RemoveAt(0);
                dontLoad.Add(wc);
            }
            yield return new WaitForSeconds(0);
        }
        LoadChunks = false;
    }
    IEnumerator UnloadGraphics()
    {
        while (unloadGraphics.Count > 0)
        {
            Chunk c = unloadGraphics[0];
            loadedGraphics.Remove(c);
            unloadGraphics.RemoveAt(0);
            c.Unload();
            chunkPool.Add(c);
            yield return new WaitForSeconds(0);
        }
        UnloadingGraphics = false;
    }

    public bool paused = false;
    bool RepositionGraphics;

    void RunGame()
    {
        if (paused)
        {
            return;
        }
        Debug.Log(meshDataLoaded + " " + meshDataPool.Count);
        int fpser = (int)(1f / Time.unscaledDeltaTime);
        fpsAverage += fpser;
        fpsCounter++;
        fps.text = "FPS: " + fpsAverage / fpsCounter;
        _timer = Time.unscaledTime + 1f;
        fpsAverage = 0;
        fpsCounter = 0;
        memory.text = "Memory: " + (System.GC.GetTotalMemory(false) / 1000000000.0);
        if (Time.unscaledTime > _timer)
        {
        }
        if (!loading && !LoadChunks)
        {
            thread = new Thread(LoadThread);
            thread.Start();
            loading = true;
        }
        else if (done)
        {
            thread.Join();
            position.text = "Position: ( " + (player.wp.posIndex.x) + " , " + (player.wp.posIndex.y) + " , " + (player.wp.posIndex.z) + " )";
            if (RepositionGraphics)
            {
                player.UpdateWorldPos();
                for (int i = 0; i < loadedGraphics.Count; i++)
                {
                    loadedGraphics[i].SetPosition();
                }
                RepositionGraphics = false;
            }
            if (LoadingGraphics)
            {
                loadGraphics.AddRange(graphicsToLoad);
                graphicsToLoad.Clear();
                LoadingGraphics = false;
            }
            playerPos = player.transform.position;
            playerVel = player.rb.velocity;
            if (!reloadChunks)
            {
                dontLoadCurrent.AddRange(dontLoad);
                dontLoad.Clear();
                reloadChunks = true;
            }
            if (UnloadingGraphics)
            {
                unloadGraphics.AddRange(unloadChunkGraphics);
                unloadChunkGraphics.Clear();
            }
            done = false;
            loading = false;
            LoadChunks = true;
            StartCoroutine(LoadGraphics());
            StartCoroutine(UnloadGraphics());
        }
    }

    public void ExitGame()
    {
        if (thread.IsAlive)
        {
            thread.Join();
        }
        if (!Application.isEditor)
        {
            //Application.Quit();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
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
