using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshObjects))]
public class RandomObjectsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshObjects objects = (MeshObjects)target;

        if(GUILayout.Button("Generate Assets"))
        {
            objects.GenerateAssets();
        }
    }
    
    
}
