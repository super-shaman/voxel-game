using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine.Scripting;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
public class World : MonoBehaviour
{

    public Chunk ChunkGraphics;
    public Player player;
    public TextMeshProUGUI fps;
    public TextMeshProUGUI memory;
    public TextMeshProUGUI position;
    public GameObject pauseMenu;
    public float LODSize;

    void Start()
    {
        Chunk.LODSize = LODSize;
        Time.timeScale = 1;
        if (!Application.isEditor)
        {
            GarbageCollector.incrementalTimeSliceNanoseconds = 1000000;
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
        System.Array.Sort(VoxelChunk.loadOrder, (x, y) => x.magnitude.CompareTo(y.magnitude));
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
        System.Array.Sort(VoxelChunk.loadOrderReverse, (x, y) => (x-new Vector3Int(size,size,size)).magnitude.CompareTo((y - new Vector3Int(size, size, size)).magnitude));
        WorldChunk.loadOrder = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrder[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        System.Array.Sort(WorldChunk.loadOrder, (x, y) => x.magnitude.CompareTo(y.magnitude));
        WorldChunk.loadOrderReverse = new Vector2Int[worldChunkSize * worldChunkSize];
        for (int i = 0; i < worldChunkSize; i++)
        {
            for (int ii = 0; ii < worldChunkSize; ii++)
            {
                WorldChunk.loadOrderReverse[i * worldChunkSize + ii] = new Vector2Int(i, ii);
            }
        }
        System.Array.Sort(WorldChunk.loadOrderReverse, (x, y) => (x - new Vector2Int(worldChunkSize, worldChunkSize)).magnitude.CompareTo((y - new Vector2Int(worldChunkSize, worldChunkSize)).magnitude));
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

    List<VoxelChunk> voxelPool = new List<VoxelChunk>();
    List<int> voxelIndexPool = new List<int>();

    public void UnloadVoxelChunk(VoxelChunk chunk)
    {
        voxelIndexPool.Add(chunk.getMemIndex());
    }

    public VoxelChunk GetVoxelChunk()
    {
        //return new VoxelChunk(size);
        if (voxelIndexPool.Count == 0)
        {
            VoxelChunk v = new VoxelChunk(size);
            voxelPool.Add(v);
            return v;
        }else
        {
            int ind = voxelIndexPool[voxelIndexPool.Count - 1];
            voxelIndexPool.RemoveAt(voxelIndexPool.Count - 1);
            VoxelChunk chunk = voxelPool[ind];
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
                WorldChunk worldChunk = new WorldChunk(worldChunkSize, size, -worldChunkLoadSize / 2 + i, -worldChunkLoadSize / 2 + ii);
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
    }
    
    List<WorldChunk> heightsToLoad = new List<WorldChunk>();
    List<WorldChunk> chunksToLoad = new List<WorldChunk>();
    List<WorldChunk> structuresToLoad = new List<WorldChunk>();
    List<WorldChunk> graphicsToLoad = new List<WorldChunk>();
    List<WorldChunk> worldsToLoad = new List<WorldChunk>();
    List<WorldChunk> loadingGraphics = new List<WorldChunk>();
    int maxThreads = System.Environment.ProcessorCount;
    int threadOffset = 2;
    List<WorldChunk> loadingChunks = new List<WorldChunk>();

    List<MeshData> meshDataPool = new List<MeshData>();
    int meshDataLoaded = 0;

    void UnloadMeshData(WorldChunk wc)
    {
        wc.meshData.Clear();
    }


    public MeshData GetMeshData()
    {
        if (meshDataPool.Count == 0)
        {
            meshDataLoaded++;
            return new MeshData();
        }
        else
        {
            int index = 0;
            MeshData md = meshDataPool[index];
            meshDataPool.RemoveAt(index);
            return md;
        }
    }

    void MoveChunks()
    {
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
                if (should)
                {
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
    bool SortNeeded = false;
    void LoadTerrain()
    {

        threader = 0;
        currentPlayerPos = new Vector2Int(player.wp.posIndex.x, player.wp.posIndex.z);
        Vector2Int pos = currentPlayerPos + new Vector2Int((int)playerPos.x, (int)playerPos.z);
        WorldChunk.pos = pos;
        float mag = (playerVel.magnitude * 4 < 1 ? 1 : playerVel.magnitude * 4);
        mag = mag > worldChunkSizer * worldChunkLoadSize / 2.5f ? worldChunkSizer * worldChunkLoadSize / 2.5f : mag;
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
            //voxelPool.Sort();
            WorldChunk.ReverseSort = true;
            loadingGraphics.Sort();
            loadGraphics.Sort();
            dontLoad.Sort();
            chunksToLoad.Sort();
            structuresToLoad.Sort();
            WorldChunk.ReverseSort = false;
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
            for (int i = dontLoad.Count - 1; i >= 0; i--)
            {
                if (dontLoad[i].unloading)
                {
                    dontLoad[i].unloading = false;
                    heightsToLoad.Add(dontLoad[i]);
                    dontLoad.RemoveAt(i);
                }
            }
            for (int i = chunksToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = chunksToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                    chunksToLoad.RemoveAt(i);
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
            MoveNeeded = false;
            for (int i = 0; i < worldsToLoad.Count; i++)
            {
                WorldChunk wc = worldsToLoad[i];
                if (wc.unloading)
                {
                    wc.unloading = false;
                    heightsToLoad.Add(wc);
                }
                else
                {
                    loadingGraphics.Add(wc);
                }
            }
            worldsToLoad.Clear();
            player.SetWorldPos(playerPos);
            for (int i = 0; i < loadedGraphics.Count; i++)
            {
                loadedGraphics[i].PositionChunk(player.wp);
            }
            RepositionGraphics = true;
            addGraphics = true;
            SortNeeded = true;
            AddGraphics = true;
        }
        threader++;
        heightsToLoad.Sort();
        threader++;
        int heightsLoaded = 0;
        for (int i = heightsToLoad.Count-1; i >= 0; i--)
        {
            WorldChunk wc = heightsToLoad[i];
            if (wc.unloading)
            {
                wc.unloading = false;
            }
            if (heightsLoaded >= 30)
            {
                break;
            }
            if (!(wc.index1 - chunkIndex.x >= -worldChunkLoadSize / 2 && wc.index1 - chunkIndex.x < worldChunkLoadSize / 2 && wc.index2 - chunkIndex.y >= -worldChunkLoadSize / 2 && wc.index2 - chunkIndex.y < worldChunkLoadSize / 2))
            {
                continue;
            }
            if (!wc.AreHeightsLoaded())
            {
                wc.AddHeights();
                heightsLoaded++;
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
                }
            }
            else if (wc.HeightsLoaded() == 9)
            {
                chunksToLoad.Add(wc);
                heightsToLoad.RemoveAt(i);
            }
        }
        for (int ii = 0; ii < loadingChunks.Count; ii++)
        {
            loadingChunks[ii].thread.Join();
        }
        loadingChunks.Clear();
        for (int i = heightsToLoad.Count - 1; i >= 0; i--)
        {
            WorldChunk wc = heightsToLoad[i];
            if (wc.HeightsLoaded() == 9)
            {
                chunksToLoad.Add(wc);
                heightsToLoad.RemoveAt(i);
            }
        }
        chunksToLoad.Sort();
        threader++;
        for (int i = chunksToLoad.Count - 1; i >= 0; i--)
        {
            WorldChunk wc = chunksToLoad[i];
            if (!(wc.index1 - chunkIndex.x >= -worldChunkLoadSize / 2 + 1 && wc.index1 - chunkIndex.x < worldChunkLoadSize / 2 - 1 && wc.index2 - chunkIndex.y >= -worldChunkLoadSize / 2 + 1 && wc.index2 - chunkIndex.y < worldChunkLoadSize / 2 - 1))
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
                }
            }
            else if (wc.ChunksLoaded() == 9)
            {
                structuresToLoad.Add(wc);
                chunksToLoad.RemoveAt(i);
            }
        }
        for (int ii = 0; ii < loadingChunks.Count; ii++)
        {
            loadingChunks[ii].thread.Join();
        }
        loadingChunks.Clear();
        for (int i = chunksToLoad.Count - 1; i >= 0; i--)
        {
            WorldChunk wc = chunksToLoad[i];
            if (wc.ChunksLoaded() == 9)
            {
                structuresToLoad.Add(wc);
                chunksToLoad.RemoveAt(i);
            }
        }
        structuresToLoad.Sort();
        threader++;
        if (!AddGraphics)
        {
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if (!(wc.index1 - chunkIndex.x >= -worldChunkLoadSize / 2 + 2 && wc.index1 - chunkIndex.x < worldChunkLoadSize / 2 - 2 && wc.index2 - chunkIndex.y >= -worldChunkLoadSize / 2 + 2 && wc.index2 - chunkIndex.y < worldChunkLoadSize / 2 - 2))
                {
                    continue;
                }
                if (!wc.AreStructuresLoaded())
                {
                    wc.AddStructures();
                    wc.LoadStructures();
                }

                if (wc.StructuresLoaded() == 9)
                {
                    worldsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                }
            }
            for (int i = structuresToLoad.Count - 1; i >= 0; i--)
            {
                WorldChunk wc = structuresToLoad[i];
                if (wc.StructuresLoaded() == 9)
                {
                    worldsToLoad.Add(wc);
                    structuresToLoad.RemoveAt(i);
                }
            }
            worldsToLoad.Sort();
            AddGraphics = true;
        }
        SortNeeded = false;
    }
    bool addGraphics;
    void RunLoadGraphics()
    {
        if (AddGraphics)
        {

            if (worldsToLoad.Count > 0)
            {
                for (int i = worldsToLoad.Count - 1; i >= 0; i--)
                {
                    WorldChunk wc = worldsToLoad[i];
                    if (wc.unloading)
                    {
                        wc.unloading = false;
                        heightsToLoad.Add(wc);
                    }
                    else
                    {
                        loadingGraphics.Add(wc);
                    }
                }
                worldsToLoad.Clear();
            }
            if (loadGraphics.Count > 0)
            {
                loadGraphics.Sort();
            }
            if (loadingGraphics.Count > 0)
            {
                loadingGraphics.Sort();
            }
            AddGraphics = false;
        }
        for (int i = 0; i < unloadMeshData.Count; i++)
        {
            MeshData md = unloadMeshData[i];
            md.Unload();
            meshDataPool.Add(md);
        }
        unloadMeshData.Clear();
        Vector2Int pos = currentPlayerPos + new Vector2Int((int)playerPos.x, (int)playerPos.z);
        if (meshDataPool.Count == meshDataLoaded)
        {
            int index = loadingGraphics.Count - 1;
            WorldChunk closest = loadingGraphics.Count > 0 ? loadingGraphics[index] : null;
            if (closest != null)
            {
                closest.meshData.Add(GetMeshData());
                closest.LoadGraphics();
                loadGraphics.Add(closest);
                loadingGraphics.RemoveAt(index);
            }
        }
        graphicsDone = true;
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
    Thread thread2;
    bool graphicsDone = false;
    bool graphicsLoading = false;
    Vector2Int chunkIndex = new Vector2Int();
    Vector2Int previousChunkIndex = new Vector2Int();
    Vector2Int chunkMover = new Vector2Int();
    Vector3 playerPos;
    Vector3 playerVel;
    List<Chunk> chunkPool = new List<Chunk>();
    List<Chunk> unloadChunks = new List<Chunk>();
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
    List<WorldChunk> dontLoad = new List<WorldChunk>();
    List<Chunk> unloadGraphics = new List<Chunk>();
    
    
    float _timer;
    float fpsAverage;
    int fpsCounter;

    List<MeshData> unloadMeshData = new List<MeshData>();
    bool LoadChunks = false;
    
    public bool paused = false;
    bool RepositionGraphics;
    bool AddGraphics = false;

    void RunGame()
    {
        if (paused)
        {
            return;
        }
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
            playerVel = player.rb.velocity;
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
            if (LoadChunks && loadGraphics.Count == 0)
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
                Chunk c = unloadGraphics[0];
                loadedGraphics.Remove(c);
                unloadGraphics.RemoveAt(0);
                c.Unload();
                chunkPool.Add(c);
            }
            if (LoadChunks && loadGraphics.Count > 0)
            {
                WorldChunk wc = loadGraphics[0];
                Chunk c = GetChunk();
                c.load(worldChunkSizer, wc, 0);
                c.PositionChunk(player.wp);
                c.SetPosition();
                loadedGraphics.Add(c);
                unloadMeshData.Add(wc.meshData[0]);
                wc.meshData.RemoveAt(0);
                if (wc.meshData.Count == 0)
                {
                    loadGraphics.RemoveAt(0);
                    dontLoad.Add(wc);
                }
            }
        }
    }



    //Sliders
    public Slider LODSlider;
    public void SetDrawDistance()
    {
        Chunk.LODSize = LODSlider.value;
        Debug.Log(Chunk.LODSize);
        for (int i = 0; i < loadedGraphics.Count; i++)
        {
            loadedGraphics[i].SetDrawDistance();
        }
    }



    public void OnApplicationQuit()
    {
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
        Chunk[] chunks = FindObjectsOfType<Chunk>();
        for (int i = 0; i < chunks.Length; i++)
        {
            Chunk c = chunks[i];
            c.Destroy();
        }
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
