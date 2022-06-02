
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{

    public MeshData()
    {
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        indices.Add(new List<ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
        vertDictionary.Add(new Dictionary<Vector3, ushort>());
    }
    public void Normalize()
    {
        for (int i = 0; i < normals.Count; i++)
        {
            //normals[i] += new Vector3(0, 0.0001f, 0);
            normals[i].Normalize();
        }
    }
    public int lod = 0;

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
        colors.Clear();
        for (int i = 0; i < vertDictionary.Count; i++)
        {
            vertDictionary[i].Clear();
        }
    }

    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Vector2> uvs = new List<Vector2>();
    public List<List<ushort>> indices = new List<List<ushort>>();
    public List<Color> colors = new List<Color>();
    public List<Dictionary<Vector3, ushort>> vertDictionary = new List<Dictionary<Vector3, ushort>>();

}