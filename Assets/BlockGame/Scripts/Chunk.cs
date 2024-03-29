
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public Chunk previous;
    public MeshFilter mf;
    public MeshRenderer mr;
    public WorldChunk chunk;
    public LODGroup lodGroup;
    public WorldPosition wp;
    public static float LODSize;
    public int memIndex = 0;
    
    public Vector3 offset = new Vector3();
    bool loaded = false;
    public bool unloading = false;
    //public bool visible = false;

    public bool Loaded()
    {
        return loaded;
    }

    public void load(int size, WorldChunk chunk, int index)
    {
        unloading = false;
        MeshData md = chunk.meshData[index];
        mf.mesh.SetVertices(md.vertices);
        mf.mesh.SetUVs(0, md.uvs);
        mf.mesh.SetNormals(md.normals);
        mf.mesh.subMeshCount = md.indices.Count;
        for (int i = 0; i < md.indices.Count; i++)
        {
            mf.mesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
        }
        offset = md.offset;
        wp = new WorldPosition(new Vector3Int(chunk.index1 * size, 0, chunk.index2 * size),new Vector3());
        lodGroup.RecalculateBounds();
        lodGroup.size = LODSize;
        mf.mesh.RecalculateBounds();
        this.chunk = chunk;
        loaded = true;
        chunk.VertexCount = md.vertices.Count;
        chunk.IndexCount = md.indices.Count;
    }
    public void loadBare(int size, WorldChunk chunk)
    {
        wp = new WorldPosition(new Vector3Int(chunk.index1 * size, 0, chunk.index2 * size), new Vector3());
        mf.mesh.subMeshCount = 7;
        lodGroup.RecalculateBounds();
        lodGroup.size = LODSize;
        offset = chunk.graphics[0].offset;
        this.chunk = chunk;
        loaded = true;
    }

    public void FinishBatch(int size, MeshData md)
    {
        mf.mesh.SetVertices(md.vertices);
        mf.mesh.SetUVs(0, md.uvs);
        mf.mesh.SetNormals(md.normals);
        mf.mesh.subMeshCount = md.indices.Count;
        for (int i = 0; i < md.indices.Count; i++)
        {
            mf.mesh.SetIndices(md.indices[i], MeshTopology.Triangles, i);
        }
        lodGroup.RecalculateBounds();
        lodGroup.size = LODSize;
        mf.mesh.RecalculateBounds();
        loaded = true;
        chunk.VertexCount = md.vertices.Count;
        chunk.IndexCount = md.indices.Count;
    }


    public void EnablePhysics()
    {
    }
    

    public void SetDrawDistance()
    {
        lodGroup.size = LODSize;
        lodGroup.localReferencePoint = new Vector3();
    }

    Vector3 pos;
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
        if (loaded)
        {
            gameObject.SetActive(false);
        }
        offset = new Vector3();
        mf.mesh.Clear();
        chunk = null;
        loaded = false;
    }

    public void Reload()
    {
        gameObject.SetActive(true);
        loaded = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        loaded = false;
    }
    
    public void Destroy()
    {
        Unload();
        DestroyImmediate(mf.mesh);
        DestroyImmediate(gameObject);
    }

    public Chunk next;
}
