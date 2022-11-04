using System;
using UnityEngine;

public class VoxelChunk : IComparable
{

    public int index1;
    public int index2;
    public int index3;
    byte size;

    byte[] types;
    public VoxelChunk[] chunks = new VoxelChunk[3 * 3 * 3];
    TerrainChunk terrain;

    public static Vector3Int[] loadOrder;
    public static Vector3Int[] loadOrderReverse;
    int memindex = 0;

    public int CompareTo(object obj)
    {
        VoxelChunk other = (VoxelChunk)obj;
        return index3.CompareTo(other.index3);
    }

    public int getMemIndex()
    {
        return memindex;
    }

    public VoxelChunk(byte size, int memIndex)
    {
        this.size = size;
        types = new byte[size * size * size];
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

    public int GetTypeFast(int i, int ii, int iii)
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
        if (chunk != null)
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


    public void LoadGraphicsUpFast()
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        if (md.offset.magnitude == 0)
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = md.voxelOffset;
            md.voxelOffset = new Vector3();
        } else
        {
            md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            md.voxelOffset *= size;
            md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.voxelOffset -= md.offset;
        }
        for (int i = 0; i < loadOrder.Length; i++)
        {
            Vector3Int v = loadOrder[i];
            if (md.vertices.Count > MeshData.maxVertices)
            {
                terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                md.voxelOffset *= size;
                md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                md.offset = md.voxelOffset;
                md.voxelOffset = new Vector3();
            }
            int type = types[v.x * size * size + v.y * size + v.z];
            if (type != 0)
            {
            }
        }
    }

    public void LoadGraphicsDownFast()
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
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
                    if (md.vertices.Count > MeshData.maxVertices)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
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
                    if (md.vertices.Count > MeshData.maxVertices)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
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

    public Vector4 GetLowResData(Vector3Int vv)
    {
        Vector4 v = new Vector4();
        for (int i = 0; i < 2; i++)
        {
            for (int ii = 0; ii < 2; ii++)
            {
                for (int iii = 0; iii < 2; iii++)
                {
                    int type = GetType(vv.x + i, vv.y + ii, vv.z + iii);
                    if (type != 0)
                    {
                        v.w += 1;
                    }
                }
            }
        }
        return v;
    }

    public static Color[] colors;
    
    public void LoadGraphicsDownLowQ()
    {
        MeshData md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
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
        for (int i = size/2 - 1; i >= 0; i--)
        {
            for (int ii = size/2 - 1; ii >= 0; ii--)
            {
                for (int iii = size/2 - 1; iii >= 0; iii--)
                {
                    Vector3Int v = new Vector3Int(i, ii, iii);
                    if (md.vertices.Count > MeshData.maxVertices)
                    {
                        terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                        md.voxelOffset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                        md.voxelOffset *= size;
                        md.voxelOffset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                        md.offset = md.voxelOffset;
                        md.voxelOffset = new Vector3();
                    }
                    int a = 0;
                    Color c = new Color(0,0,0,1);
                    for (int o = 0; o < 2; o++)
                    {
                        for (int oo = 0; oo < 2; oo++)
                        {
                            for (int ooo = 0; ooo < 2; ooo++)
                            {
                                int t = GetType(v.x + i, v.y + ii, v.z + iii);
                                if (t != 0)
                                {
                                    a++;
                                    c += colors[t];
                                }
                            }
                        }
                    }
                    if (a >= 4)
                    {
                        c = c / a;

                    }
                }
            }
        }
    }
    
}
