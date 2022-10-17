﻿
using UnityEngine;

public class BlockSpawner
{
    protected Vector3Int position;
    int iteration;
    public BlockSpawner(Vector3Int p, int i)
    {
        position = p;
        iteration = i;
    }

    public virtual int GetBlock(Vector3Int p)
    {
        return -1;
    }

}