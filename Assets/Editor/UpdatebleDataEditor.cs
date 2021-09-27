using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UpdatebleData),true)]
public class UpdatebleDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        UpdatebleData data = (UpdatebleData) target;
        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdateValues();
        }
    }
}
