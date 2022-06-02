
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

    public void load(int size, WorldChunk chunk, int index)
    {
        this.size = size;
        MeshData md = chunk.meshData[index];
        //mf.mesh.MarkDynamic();
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
        //mf.mesh.RecalculateNormals();
        if (physics && mf.mesh.vertices.Length > 0 && indexCount > 0)
        {
            if (colliderMesh == null)
            {
                colliderMesh = new Mesh();
                //colliderMesh.MarkDynamic();
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
        wp = new WorldPosition(new Vector3Int(chunk.index1 * size, 0, chunk.index2 * size),new Vector3());
        //transform.position = new Vector3(chunk.index1 * size, 0, chunk.index2 * size);
        //pos = transform.position;
        chunk.graphics.Add(this);
        chunk.graphics[chunk.graphics.Count-1].lodGroup.size = LODSize;
        chunk.graphics[chunk.graphics.Count - 1].lodGroup.localReferencePoint = new Vector3();
        this.chunk = chunk;
    }

    public void PositionChunk(WorldPosition pl)
    {
        pos = pl.Distance(wp);
    }

    public void SetPosition()
    {
        transform.position = pos;
    }

    public void Unload()
    {
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
    
}
