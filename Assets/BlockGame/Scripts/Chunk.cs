using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public VoxelChunk chunk;
    
    public void load(int size, WorldChunk chunk)
    {
        mf.mesh.SetVertices(chunk.vertices);
        mf.mesh.SetUVs(0,chunk.uvs);
        mf.mesh.SetColors(chunk.colors);
        mf.mesh.subMeshCount = chunk.indices.Count;
        for (int i = 0; i < chunk.indices.Count; i++)
        {
            mf.mesh.SetIndices(chunk.indices[i], MeshTopology.Triangles, i);
        }
        mf.mesh.SetNormals(chunk.normals);
        if (mf.mesh.vertices.Length > 0)
        {
            mc.sharedMesh = mf.mesh;
        }
        transform.position = new Vector3(chunk.index1 * size, 0, chunk.index2 * size);
        chunk.graphics = this;
    }

    public void Unload()
    {
        mf.mesh.Clear();
        mc.sharedMesh = null;
        gameObject.SetActive(false);
    }

    public void Reload()
    {
        gameObject.SetActive(true);
    }

    void Update()
    {

    }
    
}
