using System;
using UnityEngine;

public class VoxelChunk : IComparable
{

    public int index1;
    public int index2;
    public int index3;
    protected byte size;

    public byte[] types;
    public VoxelChunk[] chunks = new VoxelChunk[3 * 3 * 3];
    protected TerrainChunk terrain;
    
    int memindex = 0;
    protected byte chunkType = 0;

    public int CompareTo(object obj)
    {
        VoxelChunk other = (VoxelChunk)obj;
        return index3.CompareTo(other.index3);
    }

    public int getChunkType()
    {
        return chunkType;
    }
    public int getMemIndex()
    {
        return memindex;
    }

    public VoxelChunk(byte size, int memIndex, bool initMem)
    {
        this.size = size;
        if (initMem)
        {
            types = new byte[size * size * size];
        }
        memindex = memIndex;
    }

    public void Load(int index1, int index2, int index3, TerrainChunk terrain)
    {
        this.index1 = index1;
        this.index2 = index2;
        this.index3 = index3;
        this.terrain = terrain;
    }

    public void Unload()
    {
        terrain = null;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                for (int iii = 0; iii < 3; iii++)
                {
                    VoxelChunk chunk = chunks[i * 3 * 3 + ii * 3 + iii];
                    if (chunk != null)
                    {
                        chunk.chunks[(2 - i) * 3 * 3 + (2 - ii) * 3 + 2 - iii] = null;
                        chunks[i * 3 * 3 + ii * 3 + iii] = null;
                    }
                }
            }
        }
    }

    public void LoadVoxels()
    {
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                float height = terrain.heights[i * size + ii];
                for (int iii = 0; iii < size; iii++)
                {
                    int h = index3 * size + iii;
                    types[i * size * size + ii * size + iii] = (byte)(h < height ? height < 0 ? 1 : h < height - 1 ? 1 : 2 : h < 0 ? 6 : 0);
                }
            }
        }
    }

    public byte GetTypeFast(int i, int ii, int iii)
    {
        return types[i * size * size + ii * size + iii];
    }

    int GetType(int i, int ii, int iii)
    {
        if (i >= 0 && i < size && ii >= 0 && ii < size && iii >= 0 && iii < size)
        {
            return types[i * size * size + ii * size + iii];
        }
        int ier = i;
        int iier = ii;
        int iiier = iii;
        int o = 1;
        int oo = 1;
        int ooo = 1;
        if (ier >= size)
        {
            ier -= size;
            o++;
        }
        if (ier < 0)
        {
            ier += size;
            o--;
        }
        if (iier >= size)
        {
            iier -= size;
            oo++;
        }
        if (iier < 0)
        {
            iier += size;
            oo--;
        }
        if (iiier >= size)
        {
            iiier -= size;
            ooo++;
        }
        if (iiier < 0)
        {
            iiier += size;
            ooo--;
        }
        VoxelChunk chunk = chunks[o * 3 * 3 + oo * 3 + ooo];
        if (ier >= 0 && ier < size && iier >= 0 && iier < size && iiier >= 0 && iiier < size && chunk != null)
        {
            return chunk.types[ier * size * size + iier * size + iiier];
        }
        return -1;
    }

    public void SetType(int i, int ii, int iii, int type)
    {
        if (MeshData.solid[types[i * size * size + ii * size + iii]]) return;
        types[i * size * size + ii * size + iii] = (byte)type;
    }

    public void SetTypeFast(int i, int type)
    {
        types[i] = (byte)type;
    }
    
    public void LoadGraphicsDownFast()
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        md.scale = 1.0f;
        if (md.offset.magnitude == 0)
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = md.voxelOffset;
            md.voxelOffset = new Vector3();
        }
        else
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.voxelOffset -= md.offset;
        }
        for (int i = size - 1; i >= 0; i--)
        {
            for (int ii = size - 1; ii >= 0; ii--)
            {
                for (int iii = size - 1; iii >= 0; iii--)
                {
                    Vector3Int v = new Vector3Int(i, ii, iii);
                    if (md.vertices.Count > MeshData.maxVertices - 36)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
                        md.scale = 1.0f;
                    }
                    int type = types[v.x * size * size + v.y * size + v.z];
                    if (type != 0)
                    {
                        if (MeshData.blockShape[type] == 0)
                        {
                            md.LoadVoxel(v,type,GetType(v.x-1,v.y,v.z), GetType(v.x + 1, v.y, v.z), GetType(v.x, v.y-1, v.z), GetType(v.x, v.y+1, v.z), GetType(v.x, v.y, v.z-1), GetType(v.x, v.y, v.z+1));
                        }
                        else
                        {
                            md.LoadDiagonal2Front(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                            md.LoadDiagonal1Front(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));

                            //LoadDiagonal1Back(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                            //LoadDiagonal2Back(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                        }
                    }
                }
            }
        }
    }

    public void LoadGraphicsDownFastNoGrass()
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        md.scale = 1.0f;
        if (md.offset.magnitude == 0)
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = md.voxelOffset;
            md.voxelOffset = new Vector3();
        }
        else
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.voxelOffset -= md.offset;
        }
        for (int i = size - 1; i >= 0; i--)
        {
            for (int ii = size - 1; ii >= 0; ii--)
            {
                for (int iii = size - 1; iii >= 0; iii--)
                {
                    Vector3Int v = new Vector3Int(i, ii, iii);
                    if (md.vertices.Count > MeshData.maxVertices - 36)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
                        md.scale = 1.0f;
                    }
                    int type = types[v.x * size * size + v.y * size + v.z];
                    if (type != 0)
                    {
                        if (MeshData.blockShape[type] == 0)
                        {
                            md.LoadVoxelFast(v, type, GetType(v.x - 1, v.y, v.z), GetType(v.x + 1, v.y, v.z), GetType(v.x, v.y - 1, v.z), GetType(v.x, v.y + 1, v.z), GetType(v.x, v.y, v.z - 1), GetType(v.x, v.y, v.z + 1));
                        }
                    }
                }
            }
        }
    }
    
    int GetBlockScaled(int scale, bool fast, int scaler, Vector3Int vvv, ref MeshData md)
    {
        scale /= scaler;
        bool Nonsolid = true;
        int type = -1;
        int aa = 0;
        for (int o = 0; o < scale * scale * scale; o++)
        {
            md.supersuperLowResLoader[o].x = -1;
            md.supersuperLowResLoader[o].y = 0;
        }
        for (int o = 0; o < scale; o++)
        {
            for (int oo = 0; oo < scale; oo++)
            {
                for (int ooo = 0; ooo < scale; ooo++)
                {
                    Vector3Int vv = new Vector3Int(vvv.x + o*scaler, vvv.y + oo*scaler, vvv.z + ooo*scaler);
                    int t = GetType(vv.x, vv.y, vv.z);
                    int a = t <= 0 ? 0 : md.VisibleFaces(vv, t, GetType(vv.x - scaler, vv.y, vv.z), GetType(vv.x + scaler, vv.y, vv.z), GetType(vv.x, vv.y - scaler, vv.z), GetType(vv.x, vv.y + scaler, vv.z), GetType(vv.x, vv.y, vv.z - scaler), GetType(vv.x, vv.y, vv.z + scaler));
                    if (t > 0 && MeshData.blockShape[t] == 0 && !MeshData.fastSolid[t] && a != 0)
                    {
                        md.supersuperLowResLoader[o * scale * scale + oo * scale + ooo].x = t;
                        md.supersuperLowResLoader[o * scale * scale + oo * scale + ooo].y = a;
                    }
                    if (t > 0 && !MeshData.fastSolid[t] && a != 0)
                    {
                        type = -1;
                        Nonsolid = false;
                        if (fast)
                        {
                            return type;
                        }
                    }
                    if (t != -1 && type == -1 && MeshData.fastSolid[t])
                    {
                        type = t;
                    }
                }
            }
        }
        if (Nonsolid)
        {
            return type;
        }
        for (int o = 0; o < scale * scale * scale; o++)
        {
            Vector2Int vv = md.supersuperLowResLoader[o];
            for (int oo = o + 1; oo < scale * scale * scale; oo++)
            {
                Vector2Int vver = md.supersuperLowResLoader[oo];
                if (vv.x == vver.x)
                {
                    md.supersuperLowResLoader[o].y += vver.y;
                    md.supersuperLowResLoader[oo].x = -1;
                }
            }
        }
        for (int o = 0; o < scale * scale * scale; o++)
        {
            Vector2Int vv = md.supersuperLowResLoader[o];
            if (vv.x > 0 && vv.y > aa)
            {
                type = vv.x;
                aa = vv.y;
            }
        }
        return type;
    }
    public static int DistantSampling = 2;
    public void LoadGraphicsDownSuperSuperLowQ(int scale)
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        int scaler =  DistantSampling == 0 ? (scale >= 4 ? 2 : 1) : DistantSampling == 1 ? (scale == 8 ? 2 : 1) : 1;
        md.scale = scale;
        if (md.offset.magnitude == 0)
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = md.voxelOffset;
            md.voxelOffset = new Vector3();
        }
        else
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.voxelOffset -= md.offset;
        }
        for (int i = size / scale - 1; i >= 0; i--)
        {
            for (int ii = size / scale - 1; ii >= 0; ii--)
            {
                for (int iii = size / scale - 1; iii >= 0; iii--)
                {
                    Vector3Int v = new Vector3Int(i * scale, ii * scale, iii * scale);
                    if (md.vertices.Count > MeshData.maxVertices - 36)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
                        md.scale = scale;
                    }
                    Vector3Int vvv = v;
                    int type = GetBlockScaled(scale, false, scaler, vvv, ref md);
                    if (type > 0 && MeshData.blockShape[type] == 0)
                    {
                        int left = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(-scale, 0, 0), ref md);
                        int right = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(scale, 0, 0), ref md);
                        int back = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(0, -scale, 0), ref md);
                        int front = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(0, scale, 0), ref md);
                        int bottom = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(0, 0, -scale), ref md);
                        int top = GetBlockScaled(scale, true, scaler, vvv + new Vector3Int(0, 0, scale), ref md);
                        md.LoadVoxelFast(v, type, left, right, back, front, bottom, top);
                    }
                }
            }
        }
    }
}
