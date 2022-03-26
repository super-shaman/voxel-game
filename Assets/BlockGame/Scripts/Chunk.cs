using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public WorldChunk chunk;
    public LODGroup lodGroup;
    Mesh colliderMesh;

    int physicsCount = 5;
    bool[] loadPhysics =
    {
        true,
        true,
        true,
        true,
        true,
        false,
        false
    };

    int size;
    LODGroup lodInstance;

    public void load(int size, WorldChunk chunk, int index)
    {
        this.size = size;
        MeshData md = chunk.meshData[index];
        mf.mesh.SetVertices(md.vertices);
        mf.mesh.SetUVs(0, md.uvs);
        mf.mesh.SetNormals(md.normals);
        mf.mesh.subMeshCount = md.indices.Count;
        int indexCount = 0;
        for (int i = 0; i < md.indices.Count; i++)
        {
            mf.mesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
            if (loadPhysics[i])
            {
                indexCount += md.indices[i].Count;
            }
        }
        if (mf.mesh.vertices.Length > 0 && indexCount > 0)
        {
            if (colliderMesh == null)
            {
                colliderMesh = new Mesh();
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.subMeshCount = physicsCount;
                int counter = 0;
                for (int i = 0; i < md.indices.Count; i++)
                {
                    if (loadPhysics[i])
                    {
                        colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, counter);
                        counter++;
                    }
                }
                mc.sharedMesh = colliderMesh;
            }
            else
            {
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.subMeshCount = physicsCount;
                int counter = 0;
                for (int i = 0; i < md.indices.Count; i++)
                {
                    if (loadPhysics[i])
                    {
                        colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, counter);
                        counter++;
                    }
                }
                mc.sharedMesh = colliderMesh;
            }
        }
        transform.position = new Vector3(chunk.index1 * size, 0, chunk.index2 * size);
        pos = transform.position;
        chunk.graphics.Add(this);
        /*if (index != 0)
        {
            transform.SetParent(chunk.graphics[0].transform);
        }
        else
        {

        }
        if (!chunk.graphics[0].lodGroup.enabled)
        {
            chunk.graphics[0].lodGroup.enabled = true;
        }
        LOD[] lods = chunk.graphics[0].lodGroup.GetLODs();
        if (md.lod == 0)
        {
            LOD lod = new LOD(lods[1].screenRelativeTransitionHeight, new Renderer[] { mr });
            lods[1] = lod;
        }
        else
        {
            LOD lod = new LOD(lods[0].screenRelativeTransitionHeight, new Renderer[] { mr });
            lods[0] = lod;
        }
        chunk.graphics[0].lodGroup.SetLODs(lods);*/
        chunk.graphics[0].lodGroup.size = 56;
        chunk.graphics[0].lodGroup.localReferencePoint = new Vector3();
        this.chunk = chunk;
    }

    public void Unload()
    {
        mf.mesh.Clear();
        if (mc.sharedMesh != null)
        {
            mc.sharedMesh = null;
            colliderMesh.Clear();
        }
        /*LOD[] lods = lodGroup.GetLODs();
        lods[0].renderers = new Renderer[] { };
        lods[1].renderers = new Renderer[] { };
        lodGroup.SetLODs(lods);
        lodGroup.enabled = false;*/
        gameObject.SetActive(false);
        //transform.parent = null;
    }

    public void Reload()
    {
        gameObject.SetActive(true);
    }

    public static Vector3 PlayerPos = new Vector3();
    Vector3 pos;
    int order = 0;

    public void Sort()
    {
        order = (int)-(PlayerPos - pos).magnitude;
    }

    public void SetOrder()
    {
        //mr.sortingOrder = order;
    }
    /*public void load(int size, WorldChunk chunk, int index)
    {
        this.size = size;
        MeshData md = chunk.meshData[index];
        mf.mesh.SetVertices(md.vertices);
        mf.mesh.SetUVs(0, md.uvs);
        mf.mesh.SetNormals(md.normals);
        mf.mesh.subMeshCount = md.indices.Count;
        int indexCount = 0;
        for (int i = 0; i < md.indices.Count; i++)
        {
            mf.mesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
            if (loadPhysics[i])
            {
                indexCount += md.indices[i].Count;
            }
        }
        if (mf.mesh.vertices.Length > 0 && indexCount > 0)
        {
            if (colliderMesh == null)
            {
                colliderMesh = new Mesh();
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.subMeshCount = physicsCount;
                int counter = 0;
                for (int i = 0; i < md.indices.Count; i++)
                {
                    if (loadPhysics[i])
                    {
                        colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, counter);
                        counter++;
                    }
                }
                mc.sharedMesh = colliderMesh;
            }
            else
            {
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.subMeshCount = physicsCount;
                int counter = 0;
                for (int i = 0; i < md.indices.Count; i++)
                {
                    if (loadPhysics[i])
                    {
                        colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, counter);
                        counter++;
                    }
                }
                mc.sharedMesh = colliderMesh;
            }
        }
        transform.position = new Vector3(chunk.index1 * size, 0, chunk.index2 * size);
        pos = transform.position;
        chunk.graphics.Add(this);
        if (chunk.graphics[0].lodInstance == null)
        {
            chunk.graphics[0].lodInstance = Instantiate(lodGroup);
            chunk.graphics[0].lodInstance.transform.position = transform.position;
        }
        transform.SetParent(chunk.graphics[0].lodInstance.transform);
        LOD[] lods = chunk.graphics[0].lodInstance.GetLODs();
        if (md.lod == 0)
        {
            LOD lod = new LOD(lods[1].screenRelativeTransitionHeight, new Renderer[] { mr });
            lods[1] = lod;
        }
        else
        {
            LOD lod = new LOD(lods[0].screenRelativeTransitionHeight, new Renderer[] { mr });
            lods[0] = lod;
        }
        chunk.graphics[0].lodInstance.SetLODs(lods);
        chunk.graphics[0].lodInstance.RecalculateBounds();
        this.chunk = chunk;
    }
    
    public void Unload()
    {
        LOD[] lods = chunk.graphics[0].lodInstance.GetLODs();
        lods[1].renderers = new Renderer[0];
        lods[0].renderers = new Renderer[0];
        chunk.graphics[0].lodInstance.SetLODs(lods);
        mf.mesh.Clear();
        if (mc.sharedMesh != null)
        {
            mc.sharedMesh = null;
            colliderMesh.Clear();
        }
        gameObject.SetActive(false);
        transform.parent = null;
    }

    public void Reload()
    {
        gameObject.SetActive(true);
    }

    public static Vector3 PlayerPos = new Vector3();
    Vector3 pos;
    int order = 0;

    public void Sort()
    {
        order = (int)-(PlayerPos - pos).magnitude;
    }

    public void SetOrder()
    {
        //mr.sortingOrder = order;
    }*/
}
