using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridScript : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Grid grid = (Grid)target;
        if (GUILayout.Button("Build Object"))
        {
            float f = 0;
            for (int i = 0; i < grid.objects.Length; i++)
            {
                grid.objects[i].gameObject.transform.localPosition = new Vector3(0, f, 0);
                f -= grid.objects[i].rect.height;
            }
        }
    }
}
