using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        // inspector 发生更新 DrawDefaultInspector return true
        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdate)
                mapGenerator.DrawPreviewMapInInspect();
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.DrawPreviewMapInInspect();
        }
    }

}
