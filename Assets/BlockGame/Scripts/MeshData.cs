
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{

    public MeshData()
    {
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        indices.Add(new List<ushort>(maxVertices * 3));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
        vertDictionary.Add(new Dictionary<Vector3, ushort>(maxVertices));
    }
    public void Normalize()
    {
        for (int i = 0; i < normals.Count; i++)
        {
            normals[i].Normalize();
        }
    }
    public int lod = 0;
    public static int maxVertices = 65000;
    public void Unload()
    {
        lod = 0;
        if (vertices.Count == 0)
        {
            return;
        }
        vertices.Clear();
        normals.Clear();
        uvs.Clear();
        for (int i = 0; i < indices.Count; i++)
        {
            indices[i].Clear();
        }
        for (int i = 0; i < vertDictionary.Count; i++)
        {
            vertDictionary[i].Clear();
        }
        offset = new Vector3();
    }

    public List<Vector3> vertices = new List<Vector3>(maxVertices);
    public List<Vector3> normals = new List<Vector3>(maxVertices);
    public List<Vector2> uvs = new List<Vector2>(maxVertices);
    public List<List<ushort>> indices = new List<List<ushort>>();
    public List<Dictionary<Vector3, ushort>> vertDictionary = new List<Dictionary<Vector3, ushort>>();
    public Vector3 offset = new Vector3();

}