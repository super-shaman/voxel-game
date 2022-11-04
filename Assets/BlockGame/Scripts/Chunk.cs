
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public WorldChunk chunk;
    public LODGroup lodGroup;
    Mesh colliderMesh;
    bool physics = false;
    public WorldPosition wp;
    public static float LODSize;

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
    Vector3 offset = new Vector3();
    bool loaded = false;

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
        if (physics && mf.mesh.vertices.Length > 0 && indexCount > 0)
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
                colliderMesh.Clear();
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
                colliderMesh.Clear();
            }
        }
        offset = md.offset;
        wp = new WorldPosition(new Vector3Int(chunk.index1 * size, 0, chunk.index2 * size),new Vector3());
        lodGroup.RecalculateBounds();
        lodGroup.size = LODSize;
        mf.mesh.RecalculateBounds();
        this.chunk = chunk;
        loaded = true;
    }

    public void EnablePhysics()
    {
        if (colliderMesh == null)
        {
            colliderMesh = new Mesh();
            colliderMesh.SetVertices(mf.mesh.vertices);
            colliderMesh.subMeshCount = physicsCount;
            int counter = 0;
            for (int i = 0; i < mf.mesh.subMeshCount; i++)
            {
                if (loadPhysics[i])
                {
                    colliderMesh.SetIndices(mf.mesh.GetIndices(i), MeshTopology.Triangles, counter);
                    counter++;
                }
            }
            mc.sharedMesh = colliderMesh;
            colliderMesh.Clear();
        }
        else
        {
            colliderMesh.SetVertices(mf.mesh.vertices);
            colliderMesh.subMeshCount = physicsCount;
            int counter = 0;
            for (int i = 0; i < mf.mesh.subMeshCount; i++)
            {
                if (loadPhysics[i])
                {
                    colliderMesh.SetIndices(mf.mesh.GetIndices(i), MeshTopology.Triangles, counter);
                    counter++;
                }
            }
            mc.sharedMesh = colliderMesh;
            colliderMesh.Clear();
        }
    }
    

    public void SetDrawDistance()
    {
        lodGroup.size = LODSize;
        lodGroup.localReferencePoint = new Vector3();
    }

    Vector3 pos;
    public void PositionChunk(WorldPosition pl)
    {
        if (!loaded) return;
        pos = pl.Distance(wp);
    }

    public void SetPosition()
    {
        if (!loaded) return;
        transform.position = pos + offset;
    }

    public void Unload()
    {
        offset = new Vector3();
        mf.mesh.Clear();
        if (physics && mc.sharedMesh != null)
        {
            mc.sharedMesh = null;
        }
        gameObject.SetActive(false);
        loaded = false;
    }

    public void Reload()
    {
        gameObject.SetActive(true);
    }
    
    public void Destroy()
    {
        Unload();
        DestroyImmediate(mf.mesh);
        DestroyImmediate(gameObject);
    }

}
