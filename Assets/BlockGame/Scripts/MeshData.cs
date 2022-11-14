
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{

    public MeshData()
    {
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
    }
    public void Normalize()
    {
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i].Normalize();
        }
    }
    public Vector2Int[] lowResLoader = new Vector2Int[8];
    public Vector2Int[] superLowResLoader = new Vector2Int[64];
    public int lod = 0;
    public static int maxVertices = 65000;
    public void Unload()
    {
        lod = 0;
        if (vertices.Count == 0)
        {
            return;
        }
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        for (int i = 0; i < indices.Count; i++)
        {
            indices[i].Clear();
        }
        for (int i = 0; i < vertDictionary.Count; i++)
        {
            vertDictionary[i].Clear();
        }
        offset = new Vector3();
    }

    public void LoadVoxel(Vector3Int v, int type, int leftType, int rightType, int bottomType, int topType, int backType, int frontType)
    {
        int t = leftType;
        visible[0] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        t = rightType;
        visible[1] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        t = bottomType;
        visible[2] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        t = topType;
        visible[3] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        t = backType;
        visible[4] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        t = frontType;
        visible[5] = t == -1 ? false : solid[type] ? transparent[t] : fast[t] && transparent[t];
        LoadTopFast(v.x, v.y, v.z, type);
        LoadRightFast(v.x, v.y, v.z, type);
        LoadForwardFast(v.x, v.y, v.z, type);

        LoadBottomFast(v.x, v.y, v.z, type);
        LoadLeftFast(v.x, v.y, v.z, type);
        LoadBackFast(v.x, v.y, v.z, type);
    }
    public void LoadVoxelFast(Vector3Int v, int type, int leftType, int rightType, int bottomType, int topType, int backType, int frontType)
    {
        int t = leftType;
        visible[0] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = rightType;
        visible[1] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = bottomType;
        visible[2] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = topType;
        visible[3] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = backType;
        visible[4] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = frontType;
        visible[5] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        LoadTopFast(v.x, v.y, v.z, type);
        LoadRightFast(v.x, v.y, v.z, type);
        LoadForwardFast(v.x, v.y, v.z, type);

        LoadBottomFast(v.x, v.y, v.z, type);
        LoadLeftFast(v.x, v.y, v.z, type);
        LoadBackFast(v.x, v.y, v.z, type);
    }
    public void LoadVoxelFast(Vector3Int v, int type, bool leftType, bool rightType, bool backType, bool frontType, bool bottomType, bool topType)
    {
        bool t = leftType;
        visible[0] = t;
        t = rightType;
        visible[1] = t;
        t = backType;
        visible[2] = t;
        t = frontType;
        visible[3] = t;
        t = bottomType;
        visible[4] = t;
        t = topType;
        visible[5] = t;
        LoadTopFast(v.x, v.y, v.z, type);
        LoadRightFast(v.x, v.y, v.z, type);
        LoadForwardFast(v.x, v.y, v.z, type);

        LoadBottomFast(v.x, v.y, v.z, type);
        LoadLeftFast(v.x, v.y, v.z, type);
        LoadBackFast(v.x, v.y, v.z, type);
    }

    public int VisibleFaces(Vector3Int v, int type, int leftType, int rightType, int backType, int frontType, int bottomType, int topType)
    {
        int t = leftType;
        visible[0] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = rightType;
        visible[1] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = backType;
        visible[2] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = frontType;
        visible[3] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = bottomType;
        visible[4] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = topType;
        visible[5] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        int i = visible[0] ? 1 : 0;
        i += visible[1] ? 1 : 0;
        i += visible[2] ? 1 : 0;
        i += visible[3] ? 1 : 0;
        i += visible[4] ? 1 : 0;
        i += visible[5] ? 1 : 0;
        return i;
    }

    public Color GetVoxelColor(Vector3Int v, int type, int leftType, int rightType, int bottomType, int topType, int backType, int frontType)
    {
        int t = leftType;
        visible[0] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = rightType;
        visible[1] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = bottomType;
        visible[2] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = topType;
        visible[3] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = backType;
        visible[4] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        t = frontType;
        visible[5] = t == -1 ? false : solid[type] ? fastSolid[t] : fastTransparent[t];
        Color c = new Color(0,0,0,0);
        for (int i = 0; i < 6; i++)
        {
            if (visible[i])
            {
                c += colors[side[type, i]];
            }
        }
        return c;
    }

    public static Color[] colors;


    public bool[] visible = new bool[6];

    public List<Vector3> vertices = new List<Vector3>(maxVertices);
    public List<Vector3> normals = new List<Vector3>(maxVertices);
    public List<Vector2> uvs = new List<Vector2>(maxVertices);
    public List<List<ushort>> indices = new List<List<ushort>>();
    public List<Dictionary<Vector3, ushort>> vertDictionary = new List<Dictionary<Vector3, ushort>>();
    public Vector3 offset = new Vector3();

    public static int[,] side = {
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {1,0,2,2,2,2 },
        {3,3,3,3,3,3 },
        {4,4,4,4,4,4 },
        {5,5,5,5,5,5 },
        {6,6,6,6,6,6 }
    };

    public  static byte[] blockShape =
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

    public static bool[] solid =
    {
        false,
        true,
        true,
        true,
        false,
        false,
        false
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
    public  static bool[] fastSolid =
    {
        true,
        false,
        false,
        false,
        false,
        true,
        true
    };
    private static bool[] fastTransparent =
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

    public Vector3 voxelOffset;
    public float scale = 1.0f;

    ushort LoadVertex(Vector3 v, Vector3 n, Vector2 uv, int side)
    {
        side = 0;
        ushort index1;
        bool pass = vertDictionary[side].TryGetValue(v, out index1);
        if (pass)
        {
            if (Vector3.Dot(normals[index1].normalized, n) > -0.999f)
            {
                normals[index1] += n;
            }
            else
            {
                side = 1;
                index1 = 0;
                pass = vertDictionary[side].TryGetValue(v, out index1);
                if (pass)
                {
                    normals[index1] += n;
                }
                else
                {
                    index1 = (ushort)vertices.Count;
                    vertDictionary[side].Add(v, index1);
                    vertices.Add(v);
                    normals.Add(n);
                    uvs.Add(uv);
                }
            }

        }
        else
        {

            side = 1;
            ushort index2 = 0;
            pass = vertDictionary[side].TryGetValue(v, out index2);
            if (pass)
            {
                if (Vector3.Dot(normals[index2].normalized, n) > -0.999f)
                {
                    normals[index2] += n;
                    index1 = index2;
                }
                else
                {
                    index1 = (ushort)vertices.Count;
                    vertDictionary[0].Add(v, index1);
                    vertices.Add(v);
                    normals.Add(n);
                    uvs.Add(uv);
                }
            }
            else
            {
                index1 = (ushort)vertices.Count;
                vertDictionary[0].Add(v, index1);
                vertices.Add(v);
                normals.Add(n);
                uvs.Add(uv);
            }
        }
        return index1;
    }


    public void LoadDiagonal1Front(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(i + 1, iii, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii + 1, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii + 1, ii + 1) + voxelOffset + off);
            Vector3 n = new Vector3(1, 0, 1).normalized;
            normals.Add(new Vector3(1, 0, -1).normalized);
            normals.Add(new Vector3(-1, 0, 1).normalized);
            normals.Add(new Vector3(1, 1, -1).normalized);
            normals.Add(new Vector3(-1, 1, 1).normalized);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            indices[side[type, 2]].Add((ushort)index);
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    public void LoadDiagonal1Back(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(i, iii, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii + 1, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii + 1, ii) + voxelOffset + off);
            Vector3 n = new Vector3(-1, 0, -1).normalized;
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            indices[side[type, 2]].Add((ushort)index);
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    public void LoadDiagonal2Front(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(i + 1, iii, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii + 1, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii + 1, ii) + voxelOffset + off);
            Vector3 n = new Vector3(-1, 0, 1).normalized;
            normals.Add(new Vector3(1, 0, 1).normalized);
            normals.Add(new Vector3(-1, 0, -1).normalized);
            normals.Add(new Vector3(1, 1, 1).normalized);
            normals.Add(new Vector3(-1, 1, -1).normalized);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            indices[side[type, 2]].Add((ushort)index);
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    public void LoadDiagonal2Back(int i, int ii, int iii, int type, Vector3 off)
    {
        {
            int index = vertices.Count;
            vertices.Add(new Vector3(i, iii, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii, ii + 1) + voxelOffset + off);
            vertices.Add(new Vector3(i, iii + 1, ii) + voxelOffset + off);
            vertices.Add(new Vector3(i + 1, iii + 1, ii + 1) + voxelOffset + off);
            Vector3 n = new Vector3(1, 0, -1).normalized;
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            normals.Add(n);
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            indices[side[type, 2]].Add((ushort)index);
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 1));
            indices[side[type, 2]].Add((ushort)(index + 2));
            indices[side[type, 2]].Add((ushort)(index + 3));
        }
    }

    void LoadTopFast(int i, int ii, int iii, int type)
    {
        if (visible[5])
        {
            Vector3 n = new Vector3(0, 1, 0);
            Vector2 uv = new Vector2(i + voxelOffset.x, ii + voxelOffset.z);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii + scale, ii) + voxelOffset,new Vector3(visible[0] ? -1 : 0, 1, visible[2] ? -1 : 0).normalized,uv+new Vector2(0,0),0),
                LoadVertex(new Vector3(i + scale, iii + scale, ii) + voxelOffset,new Vector3(visible[1] ? 1 : 0, 1, visible[2] ? -1 : 0).normalized, uv+new Vector2(1,0),0),
                LoadVertex(new Vector3(i, iii + scale, ii + scale) + voxelOffset,new Vector3(visible[0] ? -1 : 0, 1, visible[3] ? 1 : 0).normalized,uv+new Vector2(0,1),0),
                LoadVertex(new Vector3(i + scale, iii + scale, ii + scale) + voxelOffset,new Vector3(visible[1] ? 1 : 0, 1, visible[3] ? 1 : 0).normalized,uv+new Vector2(1,1),0)
            };
            indices[side[type, 0]].Add(indexes[windingOrder[0]]);
            indices[side[type, 0]].Add(indexes[windingOrder[1]]);
            indices[side[type, 0]].Add(indexes[windingOrder[2]]);
            indices[side[type, 0]].Add(indexes[windingOrder[3]]);
            indices[side[type, 0]].Add(indexes[windingOrder[4]]);
            indices[side[type, 0]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadBottomFast(int i, int ii, int iii, int type)
    {
        if (visible[4])
        {
            Vector3 n = new Vector3(0, -1, 0);
            Vector2 uv = new Vector2(i + voxelOffset.x, ii + voxelOffset.z);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + scale, iii, ii) + voxelOffset,new Vector3(visible[1] ? 1 : 0, -1, visible[2] ? -1 : 0).normalized,uv+new Vector2(0,0),1),
                LoadVertex(new Vector3(i, iii, ii) + voxelOffset,new Vector3(visible[0] ? -1 : 0, -1, visible[2] ? -1 : 0).normalized, uv+new Vector2(-1,0),1),
                LoadVertex(new Vector3(i + scale, iii, ii + scale) + voxelOffset,new Vector3(visible[1] ? 1 : 0, -1, visible[3] ? 1 : 0).normalized,uv+new Vector2(0,1),1),
                LoadVertex(new Vector3(i, iii, ii + scale) + voxelOffset,new Vector3(visible[0] ? -1 : 0, -1, visible[3] ? 1 : 0).normalized,uv+new Vector2(-1,1),1)
            };
            indices[side[type, 1]].Add(indexes[windingOrder[0]]);
            indices[side[type, 1]].Add(indexes[windingOrder[1]]);
            indices[side[type, 1]].Add(indexes[windingOrder[2]]);
            indices[side[type, 1]].Add(indexes[windingOrder[3]]);
            indices[side[type, 1]].Add(indexes[windingOrder[4]]);
            indices[side[type, 1]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadRightFast(int i, int ii, int iii, int type)
    {
        if (visible[1])
        {
            Vector3 n = new Vector3(1, 0, 0);
            Vector2 uv = new Vector2(ii + voxelOffset.z, iii + voxelOffset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + scale, iii, ii) + voxelOffset,new Vector3(1, visible[4] ? -1 : 0, visible[2] ? -1 : 0).normalized,uv+new Vector2(0,0),2),
                LoadVertex(new Vector3(i + scale, iii, ii + scale) + voxelOffset,new Vector3(1, visible[4] ? -1 : 0, visible[3] ? 1 : 0).normalized, uv+new Vector2(1,0),2),
                LoadVertex(new Vector3(i + scale, iii + scale, ii) + voxelOffset,new Vector3(1, visible[5] ? 1 : 0, visible[2] ? -1 : 0).normalized,uv+new Vector2(0,1),2),
                LoadVertex(new Vector3(i + scale, iii + scale, ii + scale) + voxelOffset,new Vector3(1, visible[5] ? 1 : 0, visible[3] ? 1 : 0).normalized,uv+new Vector2(1,1),2)
            };
            indices[side[type, 2]].Add(indexes[windingOrder[0]]);
            indices[side[type, 2]].Add(indexes[windingOrder[1]]);
            indices[side[type, 2]].Add(indexes[windingOrder[2]]);
            indices[side[type, 2]].Add(indexes[windingOrder[3]]);
            indices[side[type, 2]].Add(indexes[windingOrder[4]]);
            indices[side[type, 2]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadLeftFast(int i, int ii, int iii, int type)
    {
        if (visible[0])
        {
            Vector3 n = new Vector3(-1, 0, 0);
            Vector2 uv = new Vector2(ii + voxelOffset.z, iii + voxelOffset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii, ii + scale) + voxelOffset,new Vector3(-1, visible[4] ? -1 : 0, visible[3] ? 1 : 0).normalized,uv+new Vector2(0,0),3),
                LoadVertex(new Vector3(i, iii, ii) + voxelOffset,new Vector3(-1, visible[4] ? -1 : 0, visible[2] ? -1 : 0).normalized, uv+new Vector2(-1,0),3),
                LoadVertex(new Vector3(i, iii + scale, ii + scale) + voxelOffset,new Vector3(-1, visible[5] ? 1 : 0, visible[3] ? 1 : 0).normalized,uv+new Vector2(0,1),3),
                LoadVertex(new Vector3(i, iii + scale, ii) + voxelOffset,new Vector3(-1, visible[5] ? 1 : 0, visible[2] ? -1 : 0).normalized,uv+new Vector2(-1,1),3)
            };
            indices[side[type, 3]].Add(indexes[windingOrder[0]]);
            indices[side[type, 3]].Add(indexes[windingOrder[1]]);
            indices[side[type, 3]].Add(indexes[windingOrder[2]]);
            indices[side[type, 3]].Add(indexes[windingOrder[3]]);
            indices[side[type, 3]].Add(indexes[windingOrder[4]]);
            indices[side[type, 3]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadForwardFast(int i, int ii, int iii, int type)
    {
        if (visible[3])
        {
            Vector3 n = new Vector3(0, 0, 1);
            Vector2 uv = new Vector2(i + voxelOffset.x, iii + voxelOffset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i + scale, iii, ii + scale) + voxelOffset,new Vector3(visible[1] ? 1 : 0, visible[4] ? -1 : 0, 1).normalized,uv+new Vector2(0,0),4),
                LoadVertex(new Vector3(i, iii, ii + scale) + voxelOffset,new Vector3(visible[0] ? -1 : 0, visible[4] ? -1 : 0, 1).normalized, uv+new Vector2(-1,0),4),
                LoadVertex(new Vector3(i + scale, iii + scale, ii + scale) + voxelOffset,new Vector3(visible[1] ? 1 : 0, visible[5] ? 1 : 0, 1).normalized,uv+new Vector2(0,1),4),
                LoadVertex(new Vector3(i, iii + scale, ii + scale) + voxelOffset,new Vector3(visible[0] ? -1 : 0, visible[5] ? 1 : 0, 1).normalized,uv+new Vector2(-1,1),4)
            };
            indices[side[type, 4]].Add(indexes[windingOrder[0]]);
            indices[side[type, 4]].Add(indexes[windingOrder[1]]);
            indices[side[type, 4]].Add(indexes[windingOrder[2]]);
            indices[side[type, 4]].Add(indexes[windingOrder[3]]);
            indices[side[type, 4]].Add(indexes[windingOrder[4]]);
            indices[side[type, 4]].Add(indexes[windingOrder[5]]);
        }
    }

    void LoadBackFast(int i, int ii, int iii, int type)
    {
        if (visible[2])
        {
            Vector3 n = new Vector3(0, 0, -1);
            Vector2 uv = new Vector2(i + voxelOffset.x, iii + voxelOffset.y);
            ushort[] indexes = {
                LoadVertex(new Vector3(i, iii, ii) + voxelOffset,new Vector3(visible[0] ? -1 : 0, visible[4] ? -1 : 0, -1).normalized,uv+new Vector2(0,0),5),
                LoadVertex(new Vector3(i + scale, iii, ii) + voxelOffset,new Vector3(visible[1] ? 1 : 0, visible[4] ? -1 : 0, -1).normalized, uv+new Vector2(1,0),5),
                LoadVertex(new Vector3(i, iii + scale, ii) + voxelOffset,new Vector3(visible[0] ? -1 : 0, visible[5] ? 1 : 0, -1).normalized,uv+new Vector2(0,1),5),
                LoadVertex(new Vector3(i + scale, iii + scale, ii) + voxelOffset,new Vector3(visible[1] ? 1 : 0, visible[5] ? 1 : 0, -1).normalized,uv+new Vector2(1,1),5)
            };
            indices[side[type, 5]].Add(indexes[windingOrder[0]]);
            indices[side[type, 5]].Add(indexes[windingOrder[1]]);
            indices[side[type, 5]].Add(indexes[windingOrder[2]]);
            indices[side[type, 5]].Add(indexes[windingOrder[3]]);
            indices[side[type, 5]].Add(indexes[windingOrder[4]]);
            indices[side[type, 5]].Add(indexes[windingOrder[5]]);
        }
    }

}