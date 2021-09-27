using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneatorEditor : Editor
{
    private Editor noiseEditor;
    private Editor terrainEditor;
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator) target;
        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }
        
        DrawSettingsEditor(mapGen.noiseData, mapGen.DrawMapInEditor, ref mapGen.noiseDataFoldout, ref noiseEditor);
        DrawSettingsEditor(mapGen.terrainData, mapGen.DrawMapInEditor, ref mapGen.terrainDataFoldout, ref terrainEditor);

        if (GUILayout.Button(("Random Seed")))
        {
            mapGen.noiseData.seed = Random.Range(1000, 99999);
            mapGen.DrawMapInEditor();
        }
        
        if (GUILayout.Button(("Generate Map")))
        {
            mapGen.DrawMapInEditor();
        }
    }
    
    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }
}
