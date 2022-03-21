using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MeshData
{

    public MeshData()
    {
        indices.Add(new List<int>());
        indices.Add(new List<int>());
        indices.Add(new List<int>());
        indices.Add(new List<int>());
        indices.Add(new List<int>());
        indices.Add(new List<int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
        vertDictionary.Add(new Dictionary<Vector3, int>());
    }

    public void Unload()
    {
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
    public List<List<int>> indices = new List<List<int>>();
    public List<Color> colors = new List<Color>();
    public List<Dictionary<Vector3, int>> vertDictionary = new List<Dictionary<Vector3, int>>();

}