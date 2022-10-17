
using UnityEngine;
public class TreeSpawner : BlockSpawner
{
    public TreeSpawner(Vector3Int p, int i) : base(p, i)
    {

    }
    public override int GetBlock(Vector3Int p)
    {
        p = p - position;
        int t = (p.x == 0 && p.z == 0 && p.y >= 0 && p.y < 8) ? 3 : -1;
        if (t != -1) return t;
        t = t == -1 && p.y >= 4 && p.y < 6 && p.x >= -2 && p.x < 3 && p.z >= -2 && p.z < 3 ? 4 : -1;
        if (t != -1) return t;
        t = t == -1 && p.y >= 6 && p.y < 9 && p.x >= -1 && p.x < 2 && p.z >= -1 && p.z < 2 ? 4 : -1;
        return t;
    }
}