
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshFilter mf;
    public MeshRenderer mr;
    public MeshCollider mc;
    public WorldChunk chunk;
    public LODGroup lodGroup;
    Mesh colliderMesh;
    bool physics = true;
    public WorldPosition wp;
    public float LODSize;

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
        chunk.graphics.Add(this);
        chunk.graphics[chunk.graphics.Count-1].lodGroup.size = LODSize;
        chunk.graphics[chunk.graphics.Count - 1].lodGroup.localReferencePoint = new Vector3();
        this.chunk = chunk;
    }

    public void EnableGrass()
    {

    }

    public void PositionChunk(WorldPosition pl)
    {
        pos = pl.Distance(wp);
    }

    public void SetPosition()
    {
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
    public void Destroy()
    {
        Unload();
        DestroyImmediate(mf.mesh);
        DestroyImmediate(gameObject);
    }

}
