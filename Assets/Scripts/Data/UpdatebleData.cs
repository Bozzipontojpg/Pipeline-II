using UnityEngine;

public class UpdatebleData : ScriptableObject
{
    public event System.Action onValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            NotifyOfUpdateValues();
        }
    }
    
    public void NotifyOfUpdateValues()
    {
        if (onValuesUpdated==null)
        {
            onValuesUpdated.Invoke();
        }
    }
}
