using System.Collections;
using System.Collections.Generic;
using Farm.Map;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapAllGenerator))]
public class MapAllGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("GenerateMap"))
        {
            ((MapAllGenerator)target).GenerateMap();
        }
        if (GUILayout.Button("CleanTileMap"))
        {
            ((MapAllGenerator)target).CleanTileMap();
        }
    }
}

[CustomEditor(typeof(CropItemGenerator))]
public class CropItemGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("GenerateMap"))
        {
            ((CropItemGenerator)target).GenerateMap();
        }
        if (GUILayout.Button("CleanTileMap"))
        {
            ((CropItemGenerator)target).CleanTileMap();
        }
    }
}
