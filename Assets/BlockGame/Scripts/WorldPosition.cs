using UnityEngine;

public class WorldPosition
{

    public Vector3Int posIndex;
    public Vector3 posOffset;

    public WorldPosition(Vector3Int vi, Vector3 vf)
    {
        posIndex = vi;
        posOffset = vf;
    }

    void Trim()
    {
        Vector3Int v = new Vector3Int(Mathf.FloorToInt(posOffset.x), Mathf.FloorToInt(posOffset.y), Mathf.FloorToInt(posOffset.z));
        posOffset -= v;
        posIndex += v;
    }

    public void Add(Vector3 v)
    {
        posOffset += v;
        Trim();
    }
    

    public Vector3 Distance(WorldPosition wp)
    {
        return new Vector3(wp.posIndex.x - posIndex.x + wp.posOffset.x - posOffset.x,
            wp.posIndex.y - posIndex.y + wp.posOffset.y - posOffset.y,
            wp.posIndex.z - posIndex.z + wp.posOffset.z - posOffset.z);
    }

}