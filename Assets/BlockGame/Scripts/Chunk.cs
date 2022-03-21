using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public VoxelChunk chunk;
    Mesh colliderMesh;

    public void load(int size, WorldChunk chunk)
    {
        MeshData md = chunk.meshData;
        mf.mesh.SetVertices(md.vertices);
        mf.mesh.SetUVs(0, md.uvs);
        mf.mesh.SetColors(md.colors);
        mf.mesh.SetNormals(md.normals);
        mf.mesh.subMeshCount = md.indices.Count;
        for (int i = 0; i < md.indices.Count; i++)
        {
            mf.mesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
        }
        /*mf.mesh.GetIndexBuffer().GetData(indices);
        flippedIndices = new int[indices.Length];
        System.Array.Copy(indices, flippedIndices, 0);
        System.Array.Reverse(indices);*/
        if (mf.mesh.vertices.Length > 0)
        {
            if (colliderMesh == null)
            {
                colliderMesh = new Mesh();
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.SetNormals(md.normals);
                colliderMesh.subMeshCount = md.indices.Count-1;
                for (int i = 0; i < md.indices.Count-1; i++)
                {
                    colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
                }
                mc.sharedMesh = colliderMesh;
            }
            else
            {
                colliderMesh.SetVertices(md.vertices);
                colliderMesh.SetNormals(md.normals);
                colliderMesh.subMeshCount = md.indices.Count-1;
                for (int i = 0; i < md.indices.Count - 1; i++)
                {
                    colliderMesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
                }
                mc.sharedMesh = colliderMesh;
            }
        }
        transform.position = new Vector3(chunk.index1 * size, 0, chunk.index2 * size);
        pos = transform.position;
        //order = (int)-(PlayerPos - pos).magnitude;
        //mr.sortingOrder = order;
        chunk.graphics = this;
    }
    
    public void Unload()
    {
        mf.mesh.Clear();
        if (mc.sharedMesh != null)
        {
            mc.sharedMesh = null;
            colliderMesh.Clear();
        }
        gameObject.SetActive(false);
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

}
