using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class VoxelChunk
{
    
    public int index1;
    public int index2;
    public int index3;
    int size;
    //public List<Vector3> vertices = new List<Vector3>();
    //public List<Vector3> normals = new List<Vector3>();
    //public List<Vector2> uvs = new List<Vector2>();
    //public List<List<int>> indices = new List<List<int>>();
    //public List<Color> colors = new List<Color>();

    byte[] types;
    public VoxelChunk[] chunks = new VoxelChunk[3 * 3 * 3];
    TerrainChunk terrain;
    public bool hasGraphics;

    public VoxelChunk(int size)
    {
        this.size = size;
        types = new byte[size * size * size];
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
                    types[i * size * size + ii * size + iii] = (byte)(h < height ? height < 0 ? 1 : h < height - 1 ? 1 : 2 : 0);
                }
            }
        }
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
        types[i * size * size + ii * size + iii] = (byte)type;
    }

    int[,] side = {
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {1,0,2,2,2,2 },
        {3,3,3,3,3,3 },
        {4,4,4,4,4,4 }
    };
    

    void LoadVoxel(int i, int ii, int iii)
    {
        int type = types[i * size * size + ii * size + iii];
        if (type == 0)
        {
            return;
        }
        if (isTransparent(GetType(i, ii, iii + 1)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i, iii + 1, ii)+offset);
            terrain.worldChunk.vertices.Add(new Vector3(i + 1, iii + 1, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii + 1, ii + 1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i + 1, iii + 1, ii + 1) + offset);
            terrain.worldChunk.normals.Add(new Vector3(0, 1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, 1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, 1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, 1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type,0]].Add(index);
            terrain.worldChunk.indices[side[type, 0]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 0]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 0]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 0]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 0]].Add(index + 3);
        }
        if (isTransparent(GetType(i, ii, iii - 1)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii, ii + 1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii + 1) + offset);
            terrain.worldChunk.normals.Add(new Vector3(0, -1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, -1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, -1, 0));
            terrain.worldChunk.normals.Add(new Vector3(0, -1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type, 1]].Add(index);
            terrain.worldChunk.indices[side[type, 1]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 1]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 1]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 1]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 1]].Add(index + 3);
        }
        if (isTransparent(GetType(i+1, ii, iii)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i + 1, iii, ii+1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii+1, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i + 1, iii+1, ii+1) + offset);
            terrain.worldChunk.normals.Add(new Vector3(1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(1, 0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type, 2]].Add(index);
            terrain.worldChunk.indices[side[type, 2]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 2]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 2]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 2]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 2]].Add(index + 3);
        }
        if (isTransparent(GetType(i - 1, ii, iii)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii+1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii+1, ii+1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii+1, ii) + offset);
            terrain.worldChunk.normals.Add(new Vector3(-1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(-1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(-1, 0, 0));
            terrain.worldChunk.normals.Add(new Vector3(-1, 0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type, 3]].Add(index);
            terrain.worldChunk.indices[side[type, 3]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 3]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 3]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 3]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 3]].Add(index + 3);
        }
        if (isTransparent(GetType(i, ii+1, iii)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii, ii + 1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii+1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii+1, ii + 1) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii+1, ii+1) + offset);
            terrain.worldChunk.normals.Add(new Vector3(0, 0, 1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, 1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, 1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type, 4]].Add(index);
            terrain.worldChunk.indices[side[type, 4]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 4]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 4]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 4]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 4]].Add(index + 3);
        }
        if (isTransparent(GetType(i, ii - 1, iii)))
        {
            int index = terrain.worldChunk.vertices.Count;
            terrain.worldChunk.vertices.Add(new Vector3(i, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i, iii+1, ii) + offset);
            terrain.worldChunk.vertices.Add(new Vector3(i+1, iii+1, ii) + offset);
            terrain.worldChunk.normals.Add(new Vector3(0, 0, -1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, -1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, -1));
            terrain.worldChunk.normals.Add(new Vector3(0, 0, -1));
            terrain.worldChunk.uvs.Add(new Vector2(0, 0));
            terrain.worldChunk.uvs.Add(new Vector2(1, 0));
            terrain.worldChunk.uvs.Add(new Vector2(0, 1));
            terrain.worldChunk.uvs.Add(new Vector2(1, 1));
            terrain.worldChunk.indices[side[type, 5]].Add(index);
            terrain.worldChunk.indices[side[type, 5]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 5]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 5]].Add(index + 1);
            terrain.worldChunk.indices[side[type, 5]].Add(index + 2);
            terrain.worldChunk.indices[side[type, 5]].Add(index + 3);
        }
    }

    bool isTransparent(int type)
    {
        return type == 0 || type == 4;
    }

    int[,] indexRotator =
    {
        { 0,2,1,1,2,3 },
        { 1,0,3,3,0,2 },
        { 3,1,2,2,1,0 },
        { 2,0,3,3,0,1 }
    };

    int rotateIndex(int r, int index)
    {
        return indexRotator[r,index];
    }
    Vector3 offset;
    public void LoadGraphics()
    {
        offset = new Vector3(index1 - terrain.worldChunk.index1*World.world.worldChunkSize, index3, index2 - terrain.worldChunk.index2 * World.world.worldChunkSize);
        offset *= size;
        for (int i = 0; i < size; i++)
        {
            for (int ii = 0; ii < size; ii++)
            {
                for (int iii = 0; iii < size; iii++)
                {
                    LoadVoxel(i, ii, iii);
                }
            }
        }
        /*if (vertices.Count > 0)
        {
            hasGraphics = true;
        }*/
    }

}
