using System;
using UnityEngine;

public class VoxelChunk : IComparable
{
    
    public int index1;
    public int index2;
    public int index3;
    int size;

    byte[] types;
    public VoxelChunk[] chunks = new VoxelChunk[3 * 3 * 3];
    TerrainChunk terrain;
    public bool hasGraphics;

    Vector3 offset;
    public static Vector3Int[] loadOrder;
    public static Vector3Int[] loadOrderReverse;
    MeshData md;
    int memindex = 0;
    static int memIndexer = 0;
    public int CompareTo(object obj)
    {
        VoxelChunk other = (VoxelChunk)obj;
        return index3.CompareTo(other.index3);
    }

    public int getMemIndex()
    {
        return memindex;
    }

    public VoxelChunk(int size)
    {
        this.size = size;
        types = new byte[size * size * size];
        memindex = memIndexer;
        memIndexer++;
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
                    VoxelChunk chunk = chunks[i * 3*3 + ii*3+iii];
                    if (chunk != null)
                    {
                        chunk.chunks[(2 - i) * 3*3 + (2 - ii)*3+2-iii] = null;
                        chunks[i * 3 *3+ ii*3+iii] = null;
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
        if (solid[types[i * size * size + ii * size + iii]]) return;
        types[i * size * size + ii * size + iii] = (byte)type;
    }

    public static int[,] side = {
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {1,0,2,2,2,2 },
        {3,3,3,3,3,3 },
        {4,4,4,4,4,4 },
        {5,5,5,5,5,5 },
        {6,6,6,6,6,6 }
    };

    private static byte[] blockShape =
    {
        0,
        0,
        0,
        0,
        0,
        1,
        0
    };

    private static bool[] transparent =
    {
        true,
        false,
        false,
        false,
        true,
        true,
        true
    };

    private static bool[] solid =
    {
        false,
        true,
        true,
        true,
        false,
        false,
        false
    };
    public static byte[] PhysicsBlock =
    {
        0,
        1,
        1,
        1,
        1,
        0,
        0
    };

    private static bool[] fast =
    {
        true,
        false,
        false,
        false,
        false,
        true,
        false
    };

    private static int[] windingOrder =
    {
        0,2,1,1,2,3
    };

    private static int[,] indexRotator =
    {
        { 0,2,1,1,2,3 },
        { 1,0,3,3,0,2 },
        { 3,1,2,2,1,0 },
        { 2,0,3,3,0,1 }
    };

    public static int rotateIndex(int r, int index)
    {
        return indexRotator[r, index];
    }

    bool[] visible = new bool[6];

    ushort LoadVertex(Vector3 v, Vector3 n, Vector2 uv, int side)
    {
        side = 0;
        ushort index1;
        bool pass = md.vertDictionary[side].TryGetValue(v, out index1);
        if (pass)
        {
            if (Vector3.Dot(md.normals[index1].normalized, n) > -0.001f)
            {
                md.normals[index1] += n;
            }else
            {
                side = 1;
                index1 = 0;
                pass = md.vertDictionary[side].TryGetValue(v, out index1);
                if (pass)
                {
                    md.normals[index1] += n;
                }
                else
                {
                    index1 = (ushort)md.vertices.Count;
                    md.vertDictionary[side].Add(v, index1);
                    md.vertices.Add(v);
                    md.normals.Add(n);
                    md.uvs.Add(uv);
                }
            }
            
        }
        else
        {

            side = 1;
            ushort index2 = 0;
            pass = md.vertDictionary[side].TryGetValue(v, out index2);
            if (pass)
            {
                if (Vector3.Dot(md.normals[index2].normalized, n) > -0.001f)
                {
                    md.normals[index2] += n;
                    index1 = index2;
                }else
                {
                    index1 = (ushort)md.vertices.Count;
                    md.vertDictionary[0].Add(v, index1);
                    md.vertices.Add(v);
                    md.normals.Add(n);
                    md.uvs.Add(uv);
                }
            }
            else 
            {
                index1 = (ushort)md.vertices.Count;
                md.vertDictionary[0].Add(v, index1);
                md.vertices.Add(v);
                md.normals.Add(n);
                md.uvs.Add(uv);
            }
        }
        return index1;
    }
    

    void LoadDiagonal1Front(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = md.vertices.Count;
            md.vertices.Add(new Vector3(i + 1, iii, ii) + offset+off);
            md.vertices.Add(new Vector3(i, iii, ii + 1) + offset+off);
            md.vertices.Add(new Vector3(i + 1, iii + 1, ii) + offset+off);
            md.vertices.Add(new Vector3(i, iii + 1, ii + 1) + offset+off);
            Vector3 n = new Vector3(1, 0, 1).normalized;
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.uvs.Add(new Vector2(0, 0));
            md.uvs.Add(new Vector2(1, 0));
            md.uvs.Add(new Vector2(0, 1));
            md.uvs.Add(new Vector2(1, 1));
            md.indices[side[type, 2]].Add((ushort)index);
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    void LoadDiagonal1Back(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = md.vertices.Count;
            md.vertices.Add(new Vector3(i, iii, ii + 1) + offset + off);
            md.vertices.Add(new Vector3(i + 1, iii, ii) + offset + off);
            md.vertices.Add(new Vector3(i, iii + 1, ii + 1) + offset + off);
            md.vertices.Add(new Vector3(i + 1, iii + 1, ii) + offset + off);
            Vector3 n = new Vector3(-1, 0, -1).normalized;
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.uvs.Add(new Vector2(0, 0));
            md.uvs.Add(new Vector2(1, 0));
            md.uvs.Add(new Vector2(0, 1));
            md.uvs.Add(new Vector2(1, 1));
            md.indices[side[type, 2]].Add((ushort)index);
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    void LoadDiagonal2Front(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = md.vertices.Count;
            md.vertices.Add(new Vector3(i + 1, iii, ii + 1) + offset + off);
            md.vertices.Add(new Vector3(i, iii, ii) + offset + off);
            md.vertices.Add(new Vector3(i + 1, iii + 1, ii + 1) + offset + off);
            md.vertices.Add(new Vector3(i, iii + 1, ii) + offset + off);
            Vector3 n = new Vector3(-1, 0, 1).normalized;
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.uvs.Add(new Vector2(0, 0));
            md.uvs.Add(new Vector2(1, 0));
            md.uvs.Add(new Vector2(0, 1));
            md.uvs.Add(new Vector2(1, 1));
            md.indices[side[type, 2]].Add((ushort)index);
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    void LoadDiagonal2Back(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = md.vertices.Count;
            md.vertices.Add(new Vector3(i, iii, ii) + offset + off);
            md.vertices.Add(new Vector3(i + 1, iii, ii + 1) + offset + off);
            md.vertices.Add(new Vector3(i, iii + 1, ii) + offset + off);
            md.vertices.Add(new Vector3(i + 1, iii + 1, ii + 1) + offset + off);
            Vector3 n = new Vector3(1, 0, -1).normalized;
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.normals.Add(n);
            md.uvs.Add(new Vector2(0, 0));
            md.uvs.Add(new Vector2(1, 0));
            md.uvs.Add(new Vector2(0, 1));
            md.uvs.Add(new Vector2(1, 1));
            md.indices[side[type, 2]].Add((ushort)index);
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 1));
            md.indices[side[type, 2]].Add((ushort)(index + 2));
            md.indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    void LoadTopFast(int i, int ii, int iii, int type)
    {
        if (visible[5])
        {
            Vector3 n = new Vector3(0, 1, 0);
            Vector2 uv = new Vector2(i + offset.x, ii + offset.z);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii + 1, ii) + offset,new Vector3(visible[0] ? -1 : 0, 1, visible[2] ? -1 : 0),uv+new Vector2(0,0),0),
                LoadVertex(new Vector3(i + 1, iii + 1, ii) + offset,new Vector3(visible[1] ? 1 : 0, 1, visible[2] ? -1 : 0), uv+new Vector2(1,0),0),
                LoadVertex(new Vector3(i, iii + 1, ii + 1) + offset,new Vector3(visible[0] ? -1 : 0, 1, visible[3] ? 1 : 0),uv+new Vector2(0,1),0),
                LoadVertex(new Vector3(i + 1, iii + 1, ii + 1) + offset,new Vector3(visible[1] ? 1 : 0, 1, visible[3] ? 1 : 0),uv+new Vector2(1,1),0)
            };
            md.indices[side[type, 0]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 0]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 0]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 0]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 0]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 0]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadBottomFast(int i, int ii, int iii, int type)
    {
        if (visible[4])
        {
            Vector3 n = new Vector3(0, -1, 0);
            Vector2 uv = new Vector2(i + offset.x, ii + offset.z);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + 1, iii, ii) + offset,new Vector3(visible[1] ? 1 : 0, -1, visible[2] ? -1 : 0),uv+new Vector2(0,0),1),
                LoadVertex(new Vector3(i, iii, ii) + offset,new Vector3(visible[0] ? -1 : 0, -1, visible[2] ? -1 : 0), uv+new Vector2(-1,0),1),
                LoadVertex(new Vector3(i + 1, iii, ii + 1) + offset,new Vector3(visible[1] ? 1 : 0, -1, visible[3] ? 1 : 0),uv+new Vector2(0,1),1),
                LoadVertex(new Vector3(i, iii, ii + 1) + offset,new Vector3(visible[0] ? -1 : 0, -1, visible[3] ? 1 : 0),uv+new Vector2(-1,1),1)
            };
            md.indices[side[type, 1]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 1]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 1]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 1]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 1]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 1]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadRightFast(int i, int ii, int iii, int type)
    {
        if (visible[1])
        {
            Vector3 n = new Vector3(1, 0, 0);
            Vector2 uv = new Vector2(ii + offset.z, iii + offset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + 1, iii, ii) + offset,new Vector3(1, visible[4] ? -1 : 0, visible[2] ? -1 : 0),uv+new Vector2(0,0),2),
                LoadVertex(new Vector3(i + 1, iii, ii + 1) + offset,new Vector3(1, visible[4] ? -1 : 0, visible[3] ? 1 : 0), uv+new Vector2(1,0),2),
                LoadVertex(new Vector3(i + 1, iii + 1, ii) + offset,new Vector3(1, visible[5] ? 1 : 0, visible[2] ? -1 : 0),uv+new Vector2(0,1),2),
                LoadVertex(new Vector3(i + 1, iii + 1, ii + 1) + offset,new Vector3(1, visible[5] ? 1 : 0, visible[3] ? 1 : 0),uv+new Vector2(1,1),2)
            };
            md.indices[side[type, 2]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 2]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 2]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 2]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 2]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 2]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadLeftFast(int i, int ii, int iii, int type)
    {
        if (visible[0])
        {
            Vector3 n = new Vector3(-1, 0, 0);
            Vector2 uv = new Vector2(ii + offset.z, iii + offset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii, ii + 1) + offset,new Vector3(-1, visible[4] ? -1 : 0, visible[3] ? 1 : 0),uv+new Vector2(0,0),3),
                LoadVertex(new Vector3(i, iii, ii) + offset,new Vector3(-1, visible[4] ? -1 : 0, visible[2] ? -1 : 0), uv+new Vector2(-1,0),3),
                LoadVertex(new Vector3(i, iii + 1, ii + 1) + offset,new Vector3(-1, visible[5] ? 1 : 0, visible[3] ? 1 : 0),uv+new Vector2(0,1),3),
                LoadVertex(new Vector3(i, iii + 1, ii) + offset,new Vector3(-1, visible[5] ? 1 : 0, visible[2] ? -1 : 0),uv+new Vector2(-1,1),3)
            };
            md.indices[side[type, 3]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 3]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 3]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 3]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 3]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 3]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadForwardFast(int i, int ii, int iii, int type)
    {
        if (visible[3])
        {
            Vector3 n = new Vector3(0, 0, 1);
            Vector2 uv = new Vector2(i + offset.x, iii + offset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + 1, iii, ii + 1) + offset,new Vector3(visible[1] ? 1 : 0, visible[4] ? -1 : 0, 1),uv+new Vector2(0,0),4),
                LoadVertex(new Vector3(i, iii, ii + 1) + offset,new Vector3(visible[0] ? -1 : 0, visible[4] ? -1 : 0, 1), uv+new Vector2(-1,0),4),
                LoadVertex(new Vector3(i + 1, iii + 1, ii + 1) + offset,new Vector3(visible[1] ? 1 : 0, visible[5] ? 1 : 0, 1),uv+new Vector2(0,1),4),
                LoadVertex(new Vector3(i, iii + 1, ii + 1) + offset,new Vector3(visible[0] ? -1 : 0, visible[5] ? 1 : 0, 1),uv+new Vector2(-1,1),4)
            };
            md.indices[side[type, 4]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 4]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 4]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 4]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 4]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 4]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadBackFast(int i, int ii, int iii, int type)
    {
        if (visible[2])
        {
            Vector3 n = new Vector3(0, 0, -1);
            Vector2 uv = new Vector2(i + offset.x, iii + offset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii, ii) + offset,new Vector3(visible[0] ? -1 : 0, visible[4] ? -1 : 0, -1),uv+new Vector2(0,0),5),
                LoadVertex(new Vector3(i + 1, iii, ii) + offset,new Vector3(visible[1] ? 1 : 0, visible[4] ? -1 : 0, -1), uv+new Vector2(1,0),5),
                LoadVertex(new Vector3(i, iii + 1, ii) + offset,new Vector3(visible[0] ? -1 : 0, visible[5] ? 1 : 0, -1),uv+new Vector2(0,1),5),
                LoadVertex(new Vector3(i + 1, iii + 1, ii) + offset,new Vector3(visible[1] ? 1 : 0, visible[5] ? 1 : 0, -1),uv+new Vector2(1,1),5)
            };
            md.indices[side[type, 5]].Add(indexes[windingOrder[0]]);
            md.indices[side[type, 5]].Add(indexes[windingOrder[1]]);
            md.indices[side[type, 5]].Add(indexes[windingOrder[2]]);
            md.indices[side[type, 5]].Add(indexes[windingOrder[3]]);
            md.indices[side[type, 5]].Add(indexes[windingOrder[4]]);
            md.indices[side[type, 5]].Add(indexes[windingOrder[5]]);
        }
    }


    public void LoadGraphicsUpFast()
    {
        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        if (md.offset.magnitude == 0)
        {
            offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            offset *= size;
            offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = offset;
            offset = new Vector3();
        }else
        {
            offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            offset *= size;
            offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            offset -= md.offset;
        }
        for (int i = 0; i < loadOrder.Length; i++)
        {
            Vector3Int v = loadOrder[i];
            if (md.vertices.Count > MeshData.maxVertices)
            {
                terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                offset *= size;
                offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                md.offset = offset;
                offset = new Vector3();
            }
            int type = types[v.x * size * size + v.y * size + v.z];
            if (type != 0)
            {
                if (blockShape[type] == 0)
                {
                    int t = GetType(v.x - 1, v.y, v.z);
                    visible[0] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x + 1, v.y, v.z);
                    visible[1] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y-1, v.z);
                    visible[2] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y+1, v.z);
                    visible[3] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y, v.z-1);
                    visible[4] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y, v.z+1);
                    visible[5] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                }
                else
                {
                }
            }
        }
    }

    public void LoadGraphicsDownFast()
    {
        md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
        if (md.offset.magnitude == 0)
        {
            offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            offset *= size;
            offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            md.offset = offset;
            offset = new Vector3();
        }
        else
        {
            offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
            offset *= size;
            offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
            offset -= md.offset;
        }
        for (int i = 0; i < loadOrderReverse.Length; i++)
        {
            Vector3Int v = loadOrderReverse[i];
            if (md.vertices.Count > MeshData.maxVertices)
            {
                terrain.worldChunk.meshData.Add(World.world.GetMeshData());
                md = terrain.worldChunk.meshData[terrain.worldChunk.meshData.Count - 1];
                offset = new Vector3(index1 - terrain.worldChunk.index1 * World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
                offset *= size;
                offset -= new Vector3(World.world.worldChunkSize * size / 2.0f, 0, World.world.worldChunkSize * size / 2.0f);
                md.offset = offset;
                offset = new Vector3();
            }
            int type = types[v.x * size * size + v.y * size + v.z];
            if (type != 0)
            {
                if (blockShape[type] == 0)
                {
                    int t = GetType(v.x - 1, v.y, v.z);
                    visible[0] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x + 1, v.y, v.z);
                    visible[1] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y - 1, v.z);
                    visible[2] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y + 1, v.z);
                    visible[3] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y, v.z - 1);
                    visible[4] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    t = GetType(v.x, v.y, v.z + 1);
                    visible[5] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
                    LoadTopFast(v.x, v.y, v.z, type);
                    LoadRightFast(v.x, v.y, v.z, type);
                    LoadForwardFast(v.x, v.y, v.z, type);

                    LoadBottomFast(v.x, v.y, v.z, type);
                    LoadLeftFast(v.x, v.y, v.z, type);
                    LoadBackFast(v.x, v.y, v.z, type);
                }
                else
                {
                    LoadDiagonal2Front(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                    LoadDiagonal1Front(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));

                    LoadDiagonal1Back(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                    LoadDiagonal2Back(v.x, v.y, v.z, type, new Vector3((float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 0) * 0.25f, 0, (float)WorldNoise.ValueCoherentNoise3D(index1 * size + v.x, index2 * size + v.y, index3 * size + v.z, 1) * 0.25f));
                }
            }
        }
    }
    
}
