using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;

public class World : MonoBehaviour
{

    public Chunk ChunkGraphics;
    public GameObject player;
    public TextMeshProUGUI fps;
    public TextMeshProUGUI memory;

    void Start()
    {
        QualitySettings.asyncUploadBufferSize = 512;
        QualitySettings.asyncUploadPersistentBuffer = true;
        LoadWorld();
    }

    int size = 8;
    int loadSize = 320;
    public int worldChunkSize = 4;
    int worldChunkLoadSize;
    int worldChunkSizer;

    TerrainChunk[,] terrains;
    WorldChunk[,] worldChunks;
    List<Vector2Int> closest = new List<Vector2Int>();

    public static World world;
    List<TerrainChunk> terrainPool = new List<TerrainChunk>();

    public void UnloadTerrain(TerrainChunk terrain)
    {

    }

    List<VoxelChunk> voxelPool = new List<VoxelChunk>();

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
            VoxelChunk chunk = voxelPool[voxelPool.Count - 1];
            voxelPool.RemoveAt(voxelPool.Count - 1);
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
        List<Vector2Int> positions = new List<Vector2Int>();
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                positions.Add(new Vector2Int(-worldChunkLoadSize / 2 + i, -worldChunkLoadSize / 2 + ii));
            }
        }
        while (positions.Count > 1)
        {
            int c = 0;
            for (int i = 1; i < positions.Count; i++)
            {
                if (positions[i].magnitude < positions[c].magnitude)
                {
                    c = i;
                }
            }
            closest.Add(positions[c]);
            positions.RemoveAt(c);
        }
        closest.Add(positions[0]);
        for (int i = 0; i < closest.Count; i++)
        {
            heightsToLoad.Add(worldChunks[closest[i].x+worldChunkLoadSize/2, closest[i].y+worldChunkLoadSize/2]);
        }
        //Load();
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

    void Load()
    {

        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk worldChunk = worldChunks[i, ii];
                worldChunk.LoadHeights();
            }
        }
        for (int i = 1; i < loadSize - 1; i++)
        {
            for (int ii = 1; ii < loadSize - 1; ii++)
            {
                TerrainChunk terrain = terrains[i, ii];
                terrain.LoadChunks();
            }
        }
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk worldChunk = worldChunks[i, ii];
                worldChunk.LoadStructures();
            }
        }
        for (int i = 2; i < loadSize - 2; i++)
        {
            for (int ii = 2; ii < loadSize - 2; ii++)
            {
                TerrainChunk terrain = terrains[i, ii];
                terrain.LoadGraphics();
            }
        }
        /*for (int i = 1; i < loadSize - 1; i++)
        {
            for (int ii = 1; ii < loadSize - 1; ii++)
            {
                TerrainChunk terrain = terrains[i, ii];
                for (int iii = 0; iii < terrain.loadedChunks.Count; iii++)
                {
                    VoxelChunk chunk = terrain.loadedChunks[iii];
                    if (chunk.hasGraphics)
                    {
                        Chunk c = Instantiate(ChunkGraphics);
                        c.load(size, chunk);
                    }
                }
            }
        }*/
        for (int i = 0; i < worldChunkLoadSize; i++)
        {
            for (int ii = 0; ii < worldChunkLoadSize; ii++)
            {
                WorldChunk wc = worldChunks[i, ii];
                Chunk c = Instantiate(ChunkGraphics);
                c.load(size, wc);
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
    bool MoveGraphics = false;

    void MoveChunks()
    {
        if (chunkMover.magnitude > 0)
        {
            MoveGraphics = true;
        }
        if (chunkMover.x > 0)
        {
            for (int i = previousChunkIndex.x; i < previousChunkIndex.x + chunkMover.x; i++)
            {
                for (int ii = 0; ii < worldChunkLoadSize; ii++)
                {
                    int ier = GetIndex(i);
                    int iier = GetIndex(ii);
                    WorldChunk chunk = worldChunks[ier, iier];
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
                            unloadChunkGraphics.Add(chunk.graphics);
                            chunk.graphics = null;
                        }
                        chunk.Unload();
                        unloadChunks.Add(chunk);
                        chunk.Load(index1, index2);
                        chunk.Load();
                    }
                }
            }
        }
        else
        {
            for (int i = previousChunkIndex.x + worldChunkLoadSize + chunkMover.x; i < previousChunkIndex.x + worldChunkLoadSize; i++)
            {
                for (int ii = 0; ii < worldChunkLoadSize; ii++)
                {
                    int ier = GetIndex(i);
                    int iier = GetIndex(ii);
                    WorldChunk chunk = worldChunks[ier, iier];
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
                            unloadChunkGraphics.Add(chunk.graphics);
                            chunk.graphics = null;
                        }
                        chunk.Unload();
                        unloadChunks.Add(chunk);
                        chunk.Load(index1, index2);
                        chunk.Load();
                    }
                }
            }
        }
        previousChunkIndex.x = chunkIndex.x;
        if (chunkMover.y > 0)
        {
            for (int i = previousChunkIndex.y; i < previousChunkIndex.y + chunkMover.y; i++)
            {
                for (int ii = 0; ii < worldChunkLoadSize; ii++)
                {
                    int ier = GetIndex(ii);
                    int iier = GetIndex(i);
                    WorldChunk chunk = worldChunks[ier, iier];
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
                            unloadChunkGraphics.Add(chunk.graphics);
                            chunk.graphics = null;
                        }
                        chunk.Unload();
                        unloadChunks.Add(chunk);
                        chunk.Load(index1, index2);
                        chunk.Load();
                    }
                }
            }
        }
        else
        {

            for (int i = previousChunkIndex.y + worldChunkLoadSize + chunkMover.y; i < previousChunkIndex.y + worldChunkLoadSize; i++)
            {
                for (int ii = 0; ii < worldChunkLoadSize; ii++)
                {
                    int ier = GetIndex(ii);
                    int iier = GetIndex(i);
                    WorldChunk chunk = worldChunks[ier, iier];
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
                            unloadChunkGraphics.Add(chunk.graphics);
                            chunk.graphics = null;
                        }
                        chunk.Unload();
                        unloadChunks.Add(chunk);
                        chunk.Load(index1, index2);
                        chunk.Load();
                    }
                }
            }
        }
        previousChunkIndex = chunkIndex;
        chunkMover = new Vector2Int();
    }

    bool updatePlayerPos = false;
    Vector2 currentPlayerPos = new Vector2();
    bool StopLoadingGraphics = false;
    bool RemoveGraphics = false;
    List<MeshData> meshDataPool = new List<MeshData>();

    void UnloadMeshData(WorldChunk wc)
    {
        wc.meshData.Unload();
        meshDataPool.Add(wc.meshData);
        wc.meshData = null;
    }
    int meshDataLoaded = 0;
    MeshData GetMeshData()
    {
        if (meshDataPool.Count == 0)
        {
            meshDataLoaded++;
            return new MeshData();
        }
        MeshData md = meshDataPool[0];
        meshDataPool.RemoveAt(0);
        return md;
    }


    void LoadTerrain()
    {
        if (!SortGraphics && chunkMover.magnitude > 0)
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
                loadedGraphics.Sort();
            }
            SortGraphics = true;
            while (SortGraphics)
            {
                Thread.Sleep((int)(1.0f / 60.0f * 1000));
            }
            StopLoadingGraphics = false;
            RemoveGraphics = false;
        }
        if (updatePlayerPos)
        {
            currentPlayerPos = playerPos;
            updatePlayerPos = false;
        }
        if (reloadChunks)
        {
            for (int i = 0; i < dontLoadCurrent.Count; i++)
            {
                if (dontLoadCurrent[i].meshData != null)
                {
                    UnloadMeshData(dontLoadCurrent[i]);
                }
                if (dontLoadCurrent[i].unloading)
                {
                    heightsToLoad.Add(dontLoadCurrent[i]);
                    dontLoadCurrent.RemoveAt(i);
                    i--;
                }
            }
            reloadChunks = false;
        }
        Vector2 pos = currentPlayerPos;
        if (!LoadingGraphics)
        {
            if ((MoveGraphics))
            {
                MoveGraphics = false;
                RemoveGraphics = false;
                StopLoadingGraphics = true;
                while (StopLoadingGraphics)
                {
                    Thread.Sleep((int)(1.0f / 60.0f * 1000));
                    if (RemoveGraphics)
                    {
                        for (int i = 0; i < unloadChunks.Count; i++)
                        {
                            WorldChunk wc = unloadChunks[i];
                            int index = loadGraphics.IndexOf(wc);
                            if (index >= 0)
                            {
                                if (dontLoadCurrent[i].meshData != null)
                                {
                                    UnloadMeshData(dontLoadCurrent[i]);
                                }
                                dontLoad.Add(wc);
                                loadGraphics.RemoveAt(index);
                            }
                        }
                        unloadChunks.Clear();
                        for (int i = 0; i < loadGraphics.Count; i++)
                        {
                            WorldChunk wc = loadGraphics[i];
                            for (int ii = i; ii < loadGraphics.Count; ii++)
                            {
                                WorldChunk wc2 = loadGraphics[ii];
                                if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (new Vector2(wc2.index1 * size * worldChunkSize, wc2.index2 * size * worldChunkSize) - pos).magnitude)
                                {
                                    loadGraphics[i] = wc2;
                                    loadGraphics[ii] = wc;
                                    wc = wc2;
                                }
                            }
                        }
                        RemoveGraphics = false;
                        StopLoadingGraphics = false;
                    }
                }
            }
            for (int i = 0; i < worldsToLoad.Count; i++)
            {
                WorldChunk wc = worldsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    worldsToLoad.RemoveAt(i);
                    i--;
                    heightsToLoad.Add(wc);
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
            int chunkLoadAmount = 64;
            int amountLoaded = 0;
            for (int i = 0; i < worldsToLoad.Count; i++)
            {
                if (amountLoaded >= chunkLoadAmount | !(meshDataLoaded-meshDataPool.Count < 64))
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
                    wc.meshData = GetMeshData();
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
                    wc.meshData = GetMeshData();
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
            loadingChunks.Clear();
            LoadingGraphics = true;
            if (graphicsToLoad.Count > 0 && chunkMover.magnitude == 0)
            {
                return;
            }
        }
        if (!UnloadingGraphics)
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
            MoveChunks();
            UnloadingGraphics = true;
            StopLoadingGraphics = false;
            RemoveGraphics = false;
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                WorldChunk wc = heightsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                }
            }
            for (int i = 0; i < heightsToLoad.Count; i++)
            {
                WorldChunk wc = heightsToLoad[i];
                if ((new Vector2(wc.index1 * size * worldChunkSize, wc.index2 * size * worldChunkSize) - pos).magnitude > (worldChunkLoadSize+4) * worldChunkSize * size / 2.0f)
                {
                    continue;
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
                if (!wc.AreStructuresLoaded())
                {
                    wc.AddStructures();
                    wc.LoadStructures();
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
        }
    }

    public void LoadThread()
    {
        while (true)
        {
            LoadTerrain();
        }
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
    bool moved = false;
    Vector2Int chunkIndex = new Vector2Int();
    Vector2Int previousChunkIndex = new Vector2Int();
    Vector2Int chunkMover = new Vector2Int();
    Vector2 playerPos;
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
    
    int loadAmount = 1;
    List<WorldChunk> loadGraphics = new List<WorldChunk>();
    bool reloadChunks = false;
    List<WorldChunk> dontLoad = new List<WorldChunk>();
    List<WorldChunk> dontLoadCurrent = new List<WorldChunk>();
    bool runningGraphicsCoroutine = false;

    /*public IEnumerator LoadGraphics()
    {
        int counter = 0;
        for (int i = 0; i < loadGraphics.Count; i++)
        {
            while (StopLoadingGraphics)
            {
                RemoveGraphics = true;
                break;
            }
            WorldChunk wc = loadGraphics[0];
            loadGraphics.RemoveAt(i);
            i--;
            dontLoad.Add(wc);
            if (wc.unloading)
            {
                continue;
            }
            Chunk c = GetChunk();
            c.load(worldChunkSizer, wc);
            loadedGraphics.Add(c);
            counter++;
            if (counter >= loadAmount)
            {
                yield return new WaitForSeconds(0);
                counter = 0;
            }
        }
        while (StopLoadingGraphics)
        {
            RemoveGraphics = true;
            break;
        }
        runningGraphicsCoroutine = false;
        yield return null;
    }*/

    List<Chunk> unloadGraphics = new List<Chunk>();
    bool runningUnloadingCoroutine = false;

    public IEnumerator UnloadGraphics()
    {
        int counter = 0;
        for (int i = 0; i < unloadGraphics.Count; i++)
        {
            Chunk c = unloadGraphics[i];
            c.Unload();
            loadedGraphics.Remove(c);
            chunkPool.Add(c);
            counter++;
            if (counter >= loadAmount)
            {
                yield return new WaitForSeconds(0);
                counter = 0;
            }
        }
        unloadGraphics.Clear();
        runningUnloadingCoroutine = false;
    }

    bool LoadingGraphics = false;
    bool UnloadingGraphics = false;
    float _timer;
    float fpsAverage;
    int fpsCounter;
    
    void LoadGraphics()
    {
        if (!StopLoadingGraphics)
        {
            if (loadGraphics.Count > 0)
            {
                WorldChunk wc = loadGraphics[0];
                loadGraphics.RemoveAt(0);
                dontLoad.Add(wc);
                if (!wc.unloading)
                {
                    Chunk c = GetChunk();
                    c.load(worldChunkSizer, wc);
                }
            }
            if (unloadGraphics.Count > 0)
            {
                Chunk c = unloadGraphics[0];
                unloadGraphics.RemoveAt(0);
                c.Unload();
                chunkPool.Add(c);
            }
        }
        else
        {
            RemoveGraphics = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SortGraphics)
        {
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].SetOrder();
            }
            SortGraphics = false;
        }
        LoadGraphics();
        Debug.Log(meshDataPool.Count);
        int fpser = (int)(1f / Time.unscaledDeltaTime);
        fpsAverage += fpser;
        fpsCounter++;
        fps.text = "FPS: " + fpsAverage / fpsCounter;
        _timer = Time.unscaledTime + 1f;
        fpsAverage = 0;
        fpsCounter = 0;
        memory.text = "Memory: " + (System.GC.GetTotalMemory(false)/1000000000.0);
        if (!reloadChunks)
        {
            dontLoadCurrent.AddRange(dontLoad);
            dontLoad.Clear();
            reloadChunks = true;
        }
        if (Time.unscaledTime > _timer)
        {
        }
        if (!updatePlayerPos)
        {
            playerPos = new Vector2(player.transform.position.x, player.transform.position.z);
            updatePlayerPos = true;
        }
        if (LoadingGraphics)
        {
            loadGraphics.AddRange(graphicsToLoad);
            graphicsToLoad.Clear();
            LoadingGraphics = false;
        }
        if (UnloadingGraphics)
        {
            Vector3 playerDist = (player.transform.position - new Vector3(chunkIndex.x * worldChunkSizer, 0, chunkIndex.y * worldChunkSizer));
            chunkMover = new Vector2Int(playerDist.x > worldChunkSizer ? Mathf.FloorToInt(playerDist.x / worldChunkSizer) : playerDist.x < -worldChunkSizer ? Mathf.CeilToInt(playerDist.x / worldChunkSizer) : 0,
                playerDist.z > worldChunkSizer ? Mathf.FloorToInt(playerDist.z / worldChunkSizer) : playerDist.z < -worldChunkSizer ? Mathf.CeilToInt(playerDist.z / worldChunkSizer) : 0);
            if (moved)
            {
                moved = false;
            }
            else if (chunkMover.magnitude > 0)
            {
                chunkIndex += new Vector2Int(chunkMover.x, chunkMover.y);
                moved = true;
            }
            unloadGraphics.AddRange(unloadChunkGraphics);
            unloadChunkGraphics.Clear();
            UnloadingGraphics = false;
        }
        if (!loading)
        {
            thread = new Thread(LoadThread);
            thread.Start();
            loading = true;
        }else if (done)
        {
            thread.Join();
            done = false;
            loading = false;
        }
    }
}
