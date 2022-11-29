using System.Collections.Generic;
using UnityEngine;
using System;

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
        voxelChunks = new VoxelChunk[this.chunkHeight + chunkDepth];
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


    public void LoadHeights()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                float height = GetHeight(index1 * size + i, index2 * size + ii,0)*(1024-16);
                heights[i * size + ii] = height;
            }
        }
    }
    public double GetOctaveNoise(int o, double x, double y)
    {
        double height = 0;
        double max = 0;
        double amount = 1;
        for (int iii = 0; iii < o; iii++)
        {
            height += WorldNoise.ValueCoherentNoise3D(x / amount, y / amount, seeder, 0) * amount;
            seeder++;
            max += amount;
            amount *= 2;
        }
        height /= max;
        return height;
    }
    int seeder = 0;
    public float GetHeight(double x, double y, int seed)
    {
        seeder = 0;
        double a = 1;
        double aa = 0;
        int size = 12;
        double h = 0;
        for (int i = 2; i <= size; i += 2)
        {
            h += GetOctaveNoise(i, x, y) * a;
            aa += a;
            a *= 64;// (GetOctaveNoise(i, x, y)+2)*32;
        }
        h /= aa;
        h += GetOctaveNoise(size+1, x, y)*(1.0-Math.Abs(h));
        h -= 0.125;
        h = h > 0 ? h * 1.125 : h * (1.0 - 0.125);
        double seaHeight = 0.05+(GetOctaveNoise(size-2, x,y)+1)*0.125;
        double mountainHeight = 0.25;
        seaHeight = seaHeight < mountainHeight ? mountainHeight : seaHeight;
        double her = h - mountainHeight;
        h = h > seaHeight ? h > mountainHeight ? h*h*(1-(her/(1-mountainHeight)))+h* (her / (1 - mountainHeight)): h*h:h < 0 ? h * 0.25 : (h*h) * h / seaHeight + h * 0.25 * (1.0 - h / seaHeight);
        return (float)h;
    }

    public void SortVoxelChunks()
    {
        loadedChunks.Sort();
    }

    public void LoadGraphicsClose()
    {
        for (int i = loadedChunks.Count - 1; i >= 0; i--)
        {
            VoxelChunk chunk = loadedChunks[i];
            chunk.LoadGraphicsDownFast();
        }
    }
    public void LoadGraphicsDownSuperLowQ(int scale)
    {
        for (int i = loadedChunks.Count - 1; i >= 0; i--)
        {
            VoxelChunk chunk = loadedChunks[i];
            chunk.LoadGraphicsDownSuperSuperLowQ(scale);
        }
    }

    public VoxelChunk getVoxelChunk(int iii)
    {
        return voxelChunks[iii + chunkDepth];
    }
    public void setVoxelChunk(VoxelChunk vc)
    {
        voxelChunks[vc.index3 + chunkDepth] = vc;
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

    public void LoadChunk(VoxelChunk chunk, int iii)
    {
        if (voxelChunks[chunkDepth + iii] != null)
        {
            return;
        }
        chunk.Load(index1, index2, iii, this);
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
    }

    void SetBlock(int i, int ii, int iii, int type)
    {
        int ier = Mathf.FloorToInt((float)i / size);
        int iier = Mathf.FloorToInt((float)ii / size);
        int iiier = Mathf.FloorToInt((float)iii / size);
        i -= ier * size;
        ii -= iier * size;
        iii -= iiier * size;
        TerrainChunk terrain = this;
        while (ier != 0 | iier != 0)
        {
            int oer = (ier > 0 ? 2 : ier < 0 ? 0 : 1);
            int ooer = (iier > 0 ? 2 : iier < 0 ? 0 : 1);
            terrain = terrain.chunks[oer * 3 + ooer];
            ier -= oer - 1;
            iier -= ooer - 1;
        }
        if (terrain.worldChunk.loadedFromDisk | !(Mathf.Abs(terrain.worldChunk.index1-worldChunk.index1) <= 1 && Mathf.Abs(terrain.worldChunk.index2 - worldChunk.index2) <= 1))
        {
            return;
        }
        VoxelChunk chunk = terrain.voxelChunks[chunkDepth + iiier];
        if (chunk != null)
        {
            if (ii < 8)
            {
                terrain.chunks[3].LoadChunk(iiier);
            }
            if (ii >= size - 8)
            {
                terrain.chunks[5].LoadChunk(iiier);
            }
            if (i >= size - 8)
            {
                terrain.chunks[7].LoadChunk(iiier);
            }
            if (i < 8)
            {
                terrain.chunks[1].LoadChunk(iiier);
            }
            if (iii >= size - 8)
            {
                terrain.LoadChunk(iiier + 1);
            }
            if (iii < 8)
            {
                terrain.LoadChunk(iiier - 1);
            }
            chunk.SetType(i, ii, iii, type);
        }else
        {
            chunk = terrain.LoadChunk(iiier);
            if (ii < 8)
            {
                terrain.chunks[3].LoadChunk(iiier);
            }
            if (ii >= size - 8)
            {
                terrain.chunks[5].LoadChunk(iiier);
            }
            if (i >= size - 8)
            {
                terrain.chunks[7].LoadChunk(iiier);
            }
            if (i < 8)
            {
                terrain.chunks[1].LoadChunk(iiier);
            }
            if (iii >= size - 8)
            {
                terrain.LoadChunk(iiier + 1);
            }
            if (iii < 8)
            {
                terrain.LoadChunk(iiier - 1);
            }
            chunk.SetType(i, ii, iii, type);
        }
    }

    void LoadBlock(int i, int ii, int iii)
    {
        int ier = Mathf.FloorToInt((float)i / size);
        int iier = Mathf.FloorToInt((float)ii / size);
        int iiier = Mathf.FloorToInt((float)iii / size);
        i -= ier * size;
        ii -= iier * size;
        iii -= iiier * size;
        TerrainChunk terrain = this;
        while (ier != 0 | iier != 0)
        {
            int oer = (ier > 0 ? 2 : ier < 0 ? 0 : 1);
            int ooer = (iier > 0 ? 2 : iier < 0 ? 0 : 1);
            terrain = terrain.chunks[oer * 3 + ooer];
            ier -= oer - 1;
            iier -= ooer - 1;
        }
        if (terrain.worldChunk.loadedFromDisk | !(Mathf.Abs(terrain.worldChunk.index1 - worldChunk.index1) <= 1 && Mathf.Abs(terrain.worldChunk.index2 - worldChunk.index2) <= 1))
        {
            return;
        }
        VoxelChunk chunk = terrain.voxelChunks[chunkDepth + iiier];
        if (chunk != null)
        {
            if (ii < 8)
            {
                terrain.chunks[3].LoadChunk(iiier);
            }
            if (ii >= size - 8)
            {
                terrain.chunks[5].LoadChunk(iiier);
            }
            if (i >= size - 8)
            {
                terrain.chunks[7].LoadChunk(iiier);
            }
            if (i < 8)
            {
                terrain.chunks[1].LoadChunk(iiier);
            }
            if (iii >= size - 8)
            {
                terrain.LoadChunk(iiier + 1);
            }
            if (iii < 8)
            {
                terrain.LoadChunk(iiier - 1);
            }

        }
        else
        {
            chunk = terrain.LoadChunk(iiier);
            if (ii < 8)
            {
                terrain.chunks[3].LoadChunk(iiier);
            }
            if (ii >= size - 8)
            {
                terrain.chunks[5].LoadChunk(iiier);
            }
            if (i >= size - 8)
            {
                terrain.chunks[7].LoadChunk(iiier);
            }
            if (i < 8)
            {
                terrain.chunks[1].LoadChunk(iiier);
            }
            if (iii >= size - 8)
            {
                terrain.LoadChunk(iiier + 1);
            }
            if (iii < 8)
            {
                terrain.LoadChunk(iiier - 1);
            }
        }
    }

    public int GetBlock(int i, int ii, int iii)
    {
        int iiier = Mathf.FloorToInt((float)iii / size);
        if (iiier < -chunkDepth || iiier >= chunkHeight)
        {
            return 0;
        }
        int ier = Mathf.FloorToInt((float)i / size);
        int iier = Mathf.FloorToInt((float)ii / size);

        i -= ier * size;
        ii -= iier * size;
        iii -= iiier * size;
        TerrainChunk terrain = this;
        while (ier != 0 | iier != 0)
        {
            int oer = (ier > 0 ? 2 : ier < 0 ? 0 : 1);
            int ooer = (iier > 0 ? 2 : iier < 0 ? 0 : 1);
            terrain = terrain.chunks[oer * 3 + ooer];
            ier -= oer - 1;
            iier -= ooer - 1;
        }
        VoxelChunk chunk = terrain.voxelChunks[chunkDepth + iiier];
        if (chunk != null)
        {
            return chunk.GetTypeFast(i, ii, iii);
        }
        else
        {
            return 0;
        }
    }

    public int GetHeight(int i, int ii)
    {
        int ier = Mathf.FloorToInt((float)i / size);
        int iier = Mathf.FloorToInt((float)ii / size);

        i -= ier * size;
        ii -= iier * size;
        TerrainChunk terrain = this;
        while (ier != 0 | iier != 0)
        {
            int oer = (ier > 0 ? 2 : ier < 0 ? 0 : 1);
            int ooer = (iier > 0 ? 2 : iier < 0 ? 0 : 1);
            terrain = terrain.chunks[oer * 3 + ooer];
            ier -= oer - 1;
            iier -= ooer - 1;
        }
        return Mathf.FloorToInt(terrain.heights[ier*size+ iier]);
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
    
    float TreeNoise(int i, int ii, int ooo, int seed, int amount)
    {
        double h = 0;
        double a = 1;
        double max = 0;
        for (int o = 0; o < amount; o++)
        {
            h += WorldNoise.ValueCoherentNoise3D((index1 * size + i)/a, (index2 * size + ii)/a, (ooo)/a, seed*8 + i)*a;
            max += a;
            a *= 2;
        }
        return (float)(h / max);
    }
    void SpawnBigTree(int i, int ii, int h, int maxs, int maxh, int seed)
    {
        float rscale = (float)(WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 0, 3)/2.0+0.5);
        int sizer = (int)(maxs);
        int height = (int)(rscale*maxh+maxh);
        Vector2 poser = new Vector2(0, 0);
        for (int ooo = 0; ooo < height; ooo++)
        {
            float s = Mathf.Lerp(sizer,0,(float)ooo / height);
            poser.x += s * TreeNoise(i,ii,ooo,0+seed*2,4)*Mathf.Clamp01(46.0f/height);
            poser.y += s * TreeNoise(i, ii, ooo, 1 + seed * 2,4) * Mathf.Clamp01(46.0f / height);
            for (int o = 0; o < (4.0f / 3.0f * Mathf.PI * s * s * s > sphere.Length ? sphere.Length : 4.0f / 3.0f * Mathf.PI * s * s * s); o++)
            {
                Vector3Int v = sphere[o];
                SetBlock(i + v.x + (int)poser.x, ii + v.y + (int)poser.y, h + ooo + v.z, 7);
            }
            if ((WorldNoise.ValueCoherentNoise3D(index1 * size + i + (int)poser.x, index2 * size + ii + (int)poser.y, ooo, 5) / 2.0 + 0.5)*4*(2.0f-(float)ooo/height) < 1)
            {
                int leaveSize = (int)(8 * ((float)(ooo > height/2 ? height-(ooo-height/2) : ooo*2) / height)* (WorldNoise.ValueCoherentNoise3D(index1 * size + i + (int)poser.x, index2 * size + ii + (int)poser.y, ooo, 8) / 2.0 + 0.5));
                int leaveIndex = (int)(4.0f / 3.0f * Mathf.PI * (leaveSize * leaveSize * leaveSize));
                Vector2Int Offset = new Vector2Int(i + (int)poser.x+(int)(WorldNoise.ValueCoherentNoise3D(index1 * size + i + (int)poser.x, index2 * size + ii + (int)poser.y, ooo, 6) * leaveSize/2), ii + (int)poser.y+(int)(WorldNoise.ValueCoherentNoise3D(index1 * size + i + (int)poser.x, index2 * size + ii + (int)poser.y, ooo, 7)* leaveSize/2));

                for (int o = 0; o < (leaveIndex > sphere.Length ? sphere.Length : leaveIndex); o++)
                {
                    Vector3Int v = sphere[o];
                    Vector2Int ver = new Vector2Int(v.x + Offset.x, v.y + Offset.y);
                    SetBlock(ver.x, ver.y, h + ooo + v.z, 8);
                }
            }
            if ((WorldNoise.ValueCoherentNoise3D(index1 * size + i+(int)poser.x, index2 * size + ii+(int)poser.y, ooo, 4) / 2.0 + 0.5)*8 < 1)
            {
                if ((int)(s / 2.0f) != 0 && (int)((height - ooo) / 2.0f) != 0)
                {
                    SpawnBigTree(i + (int)poser.x, ii + (int)poser.y, h + ooo, (int)(s / 2.0f), (int)((height - ooo) / 2.0f), seed + 1);
                }
            }
        }
    }
    void SpawnGrass(int i, int ii, int h)
    {

        SetBlock(i, ii, h + 1, 5);
    }

    public static Vector3Int[] sphere;

    public void LoadStructures()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                int height = Mathf.FloorToInt(heights[i * size + ii]);
                if (height < 0)
                {
                    int h = Mathf.CeilToInt(height);
                    for (int iii = 0; iii < -h; iii++)
                    {
                        LoadBlock(i, ii, -iii);
                    }
                }else
                {
                    int h = GetHeight(i - 1, ii);
                    int hh = height-h > 1 ? height-h : 1;
                    h = GetHeight(i + 1, ii);
                    hh = height-h > hh ? height-h : hh;
                    h = GetHeight(i, ii-1);
                    hh = height - h > hh ? height - h : hh;
                    h = GetHeight(i, ii+1);
                    hh = height - h > hh ? height - h : hh;
                    for (int iii = 0; iii < hh; iii++)
                    {
                        LoadBlock(i, ii, -iii);
                    }
                }
                if (height > 2 && (WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 0, 0) + 1) * 64 < 1)
                {
                    SpawnTree(i, ii, Mathf.FloorToInt(height));
                }else if (height > 32 && height < 256 && (WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 0, 1) + 1) * 512+ (WorldNoise.ValueCoherentNoise3D((index1 * size + i)/1000.0, (index2 * size + ii)/1000.0, 1, 1)*0.5f+0.5f) < 1)
                {
                    float s = (float)(WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 0, 3) / 2.0 + 0.5);
                    SpawnBigTree(i, ii, Mathf.FloorToInt(height), (int)(s*4.0f)+2, (int)(s*96)+16, 0);
                }
                else if (height > 1)// && (WorldNoise.ValueCoherentNoise3D(index1 * size + i, index2 * size + ii, 1, 0) + 1) * 3 < 1)
                {
                    SpawnGrass(i,ii, Mathf.FloorToInt(height));
                }
            }
        }
    }

}